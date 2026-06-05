using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanialNetAccount.Data;
using DanialNetAccount.Models;
using DanialNetAccount.ViewModels;
using DanialNetAccount.Services.Inventory;
using DanialNetAccount.Models.Enums;

namespace DanialNetAccount.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InventoryService _inventoryService;

        public InvoiceController(ApplicationDbContext context, InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices.OrderByDescending(i => i.Date).ToListAsync();
            return View(invoices);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(new InvoiceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceViewModel model)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync() ?? new GlobalSetting();

            if (settings.ClosedUntilDate.HasValue && model.Date <= settings.ClosedUntilDate.Value)
            {
                ModelState.AddModelError("", $"Transactions are locked until {settings.ClosedUntilDate.Value.ToShortDateString()}.");
            }

            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError("", "Invoice must have at least one item.");
            }
            else
            {
                // Check stock levels
                foreach (var item in model.Items)
                {
                    int stock = await _inventoryService.GetStockLevel(item.ProductId);
                    if (stock < item.Quantity)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        ModelState.AddModelError("", $"Insufficient stock for {product?.Name}. Available: {stock}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var invoice = new Invoice
                    {
                        InvoiceNumber = "INV-" + DateTime.Now.Ticks,
                        CustomerName = model.CustomerName,
                        Date = model.Date,
                        Items = model.Items!.Select(i => new InvoiceItem
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            TotalPrice = i.Quantity * i.UnitPrice
                        }).ToList()
                    };

                    invoice.TotalAmount = invoice.Items.Sum(i => i.TotalPrice);
                    invoice.DiscountAmount = 0;
                    invoice.TaxAmount = invoice.TotalAmount * 0.09m;
                    invoice.FinalAmount = invoice.TotalAmount + invoice.TaxAmount - invoice.DiscountAmount;

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    IInventoryValuationStrategy strategy = settings.ValuationMethod == InventoryValuationMethod.FIFO
                        ? new FIFOValuationStrategy()
                        : new AverageValuationStrategy();

                    decimal totalCOGS = 0;
                    foreach (var item in invoice.Items)
                    {
                        totalCOGS += await _inventoryService.SellProduct(item.ProductId, item.Quantity, strategy, invoice.Id);
                    }

                    var bankAccount = await GetOrCreateAccount("Bank", "111", AccountType.Asset, "11");
                    var salesAccount = await GetOrCreateAccount("Sales Revenue", "41", AccountType.Revenue, "4");
                    var cogsAccount = await GetOrCreateAccount("COGS", "51", AccountType.Expense, "5");
                    var inventoryAccount = await GetOrCreateAccount("Inventory", "12", AccountType.Asset, "1");
                    var taxAccount = await GetOrCreateAccount("Sales Tax Payable", "21", AccountType.Liability, "2");

                    var journalEntry = new JournalEntry
                    {
                        Date = invoice.Date,
                        Description = $"Invoice {invoice.InvoiceNumber} - {invoice.CustomerName}",
                        Lines = new List<JournalEntryLine>
                        {
                            new JournalEntryLine { AccountId = bankAccount.Id, Debit = invoice.FinalAmount, Credit = 0 },
                            new JournalEntryLine { AccountId = salesAccount.Id, Debit = 0, Credit = invoice.TotalAmount },
                            new JournalEntryLine { AccountId = taxAccount.Id, Debit = 0, Credit = invoice.TaxAmount },
                            new JournalEntryLine { AccountId = cogsAccount.Id, Debit = totalCOGS, Credit = 0 },
                            new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = 0, Credit = totalCOGS }
                        }
                    };

                    _context.JournalEntries.Add(journalEntry);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Details), new { id = invoice.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error processing invoice: " + ex.Message);
                }
            }

            ViewBag.Products = await _context.Products.ToListAsync();
            return View(model);
        }

        private async Task<Account> GetOrCreateAccount(string name, string code, AccountType type, string parentCode)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == code);
            if (account == null)
            {
                var parent = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == parentCode);
                account = new Account { Name = name, Code = code, Type = type, ParentAccountId = parent?.Id };
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
            }
            return account;
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .ThenInclude(it => it.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        public async Task<IActionResult> Download(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .ThenInclude(it => it.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            string html = $@"
<html>
<head><title>Invoice {invoice.InvoiceNumber}</title></head>
<body>
    <h1>Invoice {invoice.InvoiceNumber}</h1>
    <p>Customer: {invoice.CustomerName}</p>
    <p>Date: {invoice.Date.ToShortDateString()}</p>
    <p>Status: {invoice.Status}</p>
    <table border='1'>
        <thead><tr><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr></thead>
        <tbody>";
            foreach(var item in invoice.Items) {
                html += $"<tr><td>{item.Product?.Name}</td><td>{item.Quantity}</td><td>{item.UnitPrice}</td><td>{item.TotalPrice}</td></tr>";
            }
            html += $@"
        </tbody>
    </table>
    <p>Subtotal: {invoice.TotalAmount}</p>
    <p>Tax: {invoice.TaxAmount}</p>
    <p>Final Total: {invoice.FinalAmount}</p>
</body>
</html>";
            return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html", $"Invoice_{invoice.InvoiceNumber}.html");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Void(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null || invoice.Status == InvoiceStatus.Voided) return NotFound();

            var settings = await _context.Settings.FirstOrDefaultAsync() ?? new GlobalSetting();
            if (settings.ClosedUntilDate.HasValue && invoice.Date <= settings.ClosedUntilDate.Value)
            {
                return BadRequest("Cannot void invoice in a closed period.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                invoice.Status = InvoiceStatus.Voided;

                // 1. Restore Inventory
                await _inventoryService.RestoreProduct(invoice.Id);

                // 2. Reverse Journal Entry
                var originalJournal = await _context.JournalEntries
                    .Include(j => j.Lines)
                    .FirstOrDefaultAsync(j => j.Description.Contains(invoice.InvoiceNumber));

                if (originalJournal != null)
                {
                    var reverseJournal = new JournalEntry
                    {
                        Date = DateTime.Now,
                        Description = $"VOID: {originalJournal.Description}",
                        Lines = originalJournal.Lines.Select(l => new JournalEntryLine
                        {
                            AccountId = l.AccountId,
                            Debit = l.Credit,
                            Credit = l.Debit
                        }).ToList()
                    };
                    _context.JournalEntries.Add(reverseJournal);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Error voiding invoice: " + ex.Message);
            }

            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
    }
}

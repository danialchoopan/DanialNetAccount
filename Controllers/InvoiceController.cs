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

        public async Task<IActionResult> Index(string search, InvoiceType? type)
        {
            var query = _context.Invoices.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(i => i.InvoiceNumber.Contains(search) || i.CustomerName.Contains(search));

            if (type.HasValue)
                query = query.Where(i => i.Type == type.Value);

            var invoices = await query.OrderByDescending(i => i.Date).ToListAsync();
            return View(invoices);
        }

        public async Task<IActionResult> Create(InvoiceType type = InvoiceType.Sale)
        {
            ViewBag.Products = await _context.Products.ToListAsync();
            return View(new InvoiceViewModel { Type = type });
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
            else if (model.Type == InvoiceType.Sale)
            {
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
                        InvoiceNumber = (model.Type == InvoiceType.Sale ? "SAL-" : "PUR-") + DateTime.Now.Ticks,
                        CustomerName = model.CustomerName,
                        Date = model.Date,
                        Type = model.Type,
                        Items = model.Items!.Select(i => new InvoiceItem
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            TotalPrice = i.Quantity * i.UnitPrice
                        }).ToList()
                    };

                    invoice.TotalAmount = invoice.Items.Sum(i => i.TotalPrice);
                    invoice.TaxAmount = invoice.TotalAmount * 0.09m;
                    invoice.FinalAmount = invoice.TotalAmount + invoice.TaxAmount;

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    var bankAccount = await GetOrCreateAccount("Main Bank Account", "111", AccountType.Asset, "1");
                    var salesAccount = await GetOrCreateAccount("Sales Revenue", "411", AccountType.Revenue, "4");
                    var cogsAccount = await GetOrCreateAccount("Cost of Goods Sold", "511", AccountType.Expense, "5");
                    var inventoryAccount = await GetOrCreateAccount("Inventory", "121", AccountType.Asset, "1");
                    var taxPayableAccount = await GetOrCreateAccount("Sales Tax Payable", "221", AccountType.Liability, "2");
                    var apAccount = await GetOrCreateAccount("Accounts Payable", "211", AccountType.Liability, "2");

                    if (model.Type == InvoiceType.Sale)
                    {
                        IInventoryValuationStrategy strategy = settings.ValuationMethod == InventoryValuationMethod.FIFO
                            ? new FIFOValuationStrategy()
                            : new AverageValuationStrategy();

                        decimal totalCOGS = 0;
                        foreach (var item in invoice.Items)
                        {
                            totalCOGS += await _inventoryService.SellProduct(item.ProductId, item.Quantity, strategy, invoice.Id);
                        }

                        var journalEntry = new JournalEntry
                        {
                            Date = invoice.Date,
                            Description = $"Sale Invoice {invoice.InvoiceNumber} - {invoice.CustomerName}",
                            Lines = new List<JournalEntryLine>
                            {
                                new JournalEntryLine { AccountId = bankAccount.Id, Debit = invoice.FinalAmount, Credit = 0 },
                                new JournalEntryLine { AccountId = salesAccount.Id, Debit = 0, Credit = invoice.TotalAmount },
                                new JournalEntryLine { AccountId = taxPayableAccount.Id, Debit = 0, Credit = invoice.TaxAmount },
                                new JournalEntryLine { AccountId = cogsAccount.Id, Debit = totalCOGS, Credit = 0 },
                                new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = 0, Credit = totalCOGS }
                            }
                        };
                        _context.JournalEntries.Add(journalEntry);
                    }
                    else // PURCHASE INVOICE
                    {
                        foreach (var item in invoice.Items)
                        {
                            await _inventoryService.PurchaseProduct(item.ProductId, item.Quantity, item.UnitPrice);
                        }

                        var journalEntry = new JournalEntry
                        {
                            Date = invoice.Date,
                            Description = $"Purchase Invoice {invoice.InvoiceNumber} - {invoice.CustomerName}",
                            Lines = new List<JournalEntryLine>
                            {
                                new JournalEntryLine { AccountId = inventoryAccount.Id, Debit = invoice.TotalAmount, Credit = 0 },
                                new JournalEntryLine { AccountId = taxPayableAccount.Id, Debit = invoice.TaxAmount, Credit = 0 }, // Input VAT
                                new JournalEntryLine { AccountId = bankAccount.Id, Debit = 0, Credit = invoice.FinalAmount }
                            }
                        };
                        _context.JournalEntries.Add(journalEntry);
                    }

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

            string html = $@"<html><body style='font-family:sans-serif;'><h1>{invoice.Type} Invoice {invoice.InvoiceNumber}</h1><p>Customer/Vendor: {invoice.CustomerName}</p><p>Date: {invoice.Date.ToShortDateString()}</p><h3>Items</h3><table border='1' width='100%'><tr><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr>";
            foreach(var item in invoice.Items) html += $"<tr><td>{item.Product?.Name}</td><td>{item.Quantity}</td><td>{item.UnitPrice}</td><td>{item.TotalPrice}</td></tr>";
            html += $"</table><p>Total: {invoice.FinalAmount}</p></body></html>";

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

                if (invoice.Type == InvoiceType.Sale)
                {
                    await _inventoryService.RestoreProduct(invoice.Id);
                }
                else // PURCHASE VOID
                {
                    // For simplicity, we just reduce inventory by same quantity.
                    // Production systems would check if stock is still available.
                    foreach(var item in invoice.Items)
                    {
                        // Deduct from latest batches
                        var purchases = await _context.InventoryTransactions
                            .Where(t => t.ProductId == item.ProductId && t.Type == TransactionType.Purchase && t.RemainingQuantity >= item.Quantity)
                            .OrderByDescending(t => t.Date)
                            .ToListAsync();

                        if (purchases.Any()) purchases[0].RemainingQuantity -= item.Quantity;
                    }
                }

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

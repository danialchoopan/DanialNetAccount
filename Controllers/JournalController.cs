using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanialNetAccount.Data;
using DanialNetAccount.Models;
using DanialNetAccount.ViewModels;

namespace DanialNetAccount.Controllers
{
    public class JournalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JournalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var entries = await _context.JournalEntries
                .Include(e => e.Lines)
                .ThenInclude(l => l.Account)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
            return View(entries);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Accounts = await _context.Accounts.ToListAsync();
            return View(new JournalEntryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JournalEntryViewModel model)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync() ?? new GlobalSetting();
            if (settings.ClosedUntilDate.HasValue && model.Date <= settings.ClosedUntilDate.Value)
            {
                ModelState.AddModelError("", $"Transactions are locked until {settings.ClosedUntilDate.Value.ToShortDateString()}.");
            }

            if (model.Lines == null || model.Lines.Count < 2)
            {
                ModelState.AddModelError("", "A journal entry must have at least two lines.");
            }
            else
            {
                decimal totalDebit = model.Lines.Sum(l => l.Debit);
                decimal totalCredit = model.Lines.Sum(l => l.Credit);

                if (totalDebit != totalCredit)
                {
                    ModelState.AddModelError("", "The journal entry is not balanced (Debits must equal Credits).");
                }

                if (totalDebit == 0 && totalCredit == 0)
                {
                    ModelState.AddModelError("", "Total amount cannot be zero.");
                }
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var entry = new JournalEntry
                    {
                        Date = model.Date,
                        Description = model.Description,
                        Lines = model.Lines!.Select(l => new JournalEntryLine
                        {
                            AccountId = l.AccountId,
                            Debit = l.Debit,
                            Credit = l.Credit
                        }).ToList()
                    };

                    _context.JournalEntries.Add(entry);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "An error occurred while saving the journal entry.");
                }
            }

            ViewBag.Accounts = await _context.Accounts.ToListAsync();
            return View(model);
        }
    }
}

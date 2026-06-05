using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanialNetAccount.Data;
using DanialNetAccount.Models;
using DanialNetAccount.Models.Enums;

namespace DanialNetAccount.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Accounts
                .Include(a => a.Children)
                .ToListAsync();

            var rootAccounts = accounts.Where(a => a.ParentAccountId == null).ToList();
            return View(rootAccounts);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Accounts = await _context.Accounts.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Accounts = await _context.Accounts.ToListAsync();
            return View(account);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DanialNetAccount.Data;
using DanialNetAccount.Models;

namespace DanialNetAccount.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GlobalSetting();
                _context.Settings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(GlobalSetting model)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings != null)
            {
                settings.ValuationMethod = model.ValuationMethod;
                settings.ClosedUntilDate = model.ClosedUntilDate;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

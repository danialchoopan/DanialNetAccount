using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using DanialNetAccount.Services.Reports;
using DanialNetAccount.ViewModels;

namespace DanialNetAccount.Controllers
{
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;
        private readonly IMemoryCache _cache;
        private const string TrialBalanceCacheKey = "TrialBalanceReport";
        private const string ProfitLossCacheKey = "ProfitLossReport";
        private const string BalanceSheetCacheKey = "BalanceSheetReport";

        public ReportController(ReportService reportService, IMemoryCache cache)
        {
            _reportService = reportService;
            _cache = cache;
        }

        public async Task<IActionResult> TrialBalance()
        {
            if (!_cache.TryGetValue(TrialBalanceCacheKey, out TrialBalanceReport? report))
            {
                report = await _reportService.GetTrialBalance();
                _cache.Set(TrialBalanceCacheKey, report, TimeSpan.FromMinutes(10));
            }
            return View(report);
        }

        public async Task<IActionResult> ProfitLoss()
        {
            if (!_cache.TryGetValue(ProfitLossCacheKey, out ProfitLossReport? report))
            {
                report = await _reportService.GetProfitLoss();
                _cache.Set(ProfitLossCacheKey, report, TimeSpan.FromMinutes(10));
            }
            return View(report);
        }

        public async Task<IActionResult> BalanceSheet()
        {
            if (!_cache.TryGetValue(BalanceSheetCacheKey, out BalanceSheetReport? report))
            {
                report = await _reportService.GetBalanceSheet();
                _cache.Set(BalanceSheetCacheKey, report, TimeSpan.FromMinutes(10));
            }
            return View(report);
        }

        public async Task<IActionResult> AccountLedger(int id, DateTime? fromDate, DateTime? toDate)
        {
            var report = await _reportService.GetAccountLedger(id, fromDate, toDate);
            return View(report);
        }

        public IActionResult ClearCache()
        {
            _cache.Remove(TrialBalanceCacheKey);
            _cache.Remove(ProfitLossCacheKey);
            _cache.Remove(BalanceSheetCacheKey);
            return RedirectToAction("TrialBalance");
        }
    }
}

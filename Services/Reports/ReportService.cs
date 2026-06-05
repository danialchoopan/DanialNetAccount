using DanialNetAccount.Data;
using DanialNetAccount.ViewModels;
using DanialNetAccount.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DanialNetAccount.Services.Reports
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrialBalanceReport> GetTrialBalance()
        {
            // SQLite doesn't support Sum on decimal in SQL. We fetch to memory first.
            var allLines = await _context.JournalEntryLines
                .Include(l => l.Account)
                .ToListAsync();

            var lines = allLines
                .GroupBy(l => new { l.Account!.Code, l.Account.Name })
                .Select(g => new TrialBalanceLine
                {
                    AccountCode = g.Key.Code,
                    AccountName = g.Key.Name,
                    Debit = g.Sum(x => x.Debit),
                    Credit = g.Sum(x => x.Credit)
                })
                .OrderBy(l => l.AccountCode)
                .ToList();

            return new TrialBalanceReport
            {
                Lines = lines,
                TotalDebit = lines.Sum(l => l.Debit),
                TotalCredit = lines.Sum(l => l.Credit)
            };
        }

        public async Task<ProfitLossReport> GetProfitLoss()
        {
            var allLines = await _context.JournalEntryLines
                .Include(l => l.Account)
                .ToListAsync();

            var revenue = allLines
                .Where(l => l.Account!.Type == AccountType.Revenue)
                .GroupBy(l => l.Account!.Name)
                .Select(g => new ProfitLossLine
                {
                    AccountName = g.Key,
                    Amount = g.Sum(x => x.Credit - x.Debit)
                })
                .ToList();

            var expenses = allLines
                .Where(l => l.Account!.Type == AccountType.Expense)
                .GroupBy(l => l.Account!.Name)
                .Select(g => new ProfitLossLine
                {
                    AccountName = g.Key,
                    Amount = g.Sum(x => x.Debit - x.Credit)
                })
                .ToList();

            return new ProfitLossReport
            {
                RevenueLines = revenue,
                ExpenseLines = expenses,
                TotalRevenue = revenue.Sum(r => r.Amount),
                TotalExpenses = expenses.Sum(e => e.Amount)
            };
        }

        public async Task<BalanceSheetReport> GetBalanceSheet()
        {
            var pnl = await GetProfitLoss();
            decimal netProfit = pnl.NetProfit;

            var assetLines = await GetBalanceSheetLines(AccountType.Asset);
            var liabilityLines = await GetBalanceSheetLines(AccountType.Liability);
            var equityLines = await GetBalanceSheetLines(AccountType.Equity);

            equityLines.Add(new BalanceSheetLine { AccountName = "Net Profit (Current Period)", Balance = netProfit });

            return new BalanceSheetReport
            {
                AssetLines = assetLines,
                LiabilityLines = liabilityLines,
                EquityLines = equityLines,
                TotalAssets = assetLines.Sum(l => l.Balance),
                TotalLiabilities = liabilityLines.Sum(l => l.Balance),
                TotalEquity = equityLines.Sum(l => l.Balance)
            };
        }

        private async Task<List<BalanceSheetLine>> GetBalanceSheetLines(AccountType type)
        {
            var allLines = await _context.JournalEntryLines
                .Include(l => l.Account)
                .Where(l => l.Account!.Type == type)
                .ToListAsync();

            return allLines
                .GroupBy(l => l.Account!.Name)
                .Select(g => new BalanceSheetLine
                {
                    AccountName = g.Key,
                    Balance = type == AccountType.Asset ? g.Sum(x => x.Debit - x.Credit) : g.Sum(x => x.Credit - x.Debit)
                })
                .Where(l => l.Balance != 0)
                .ToList();
        }

        public async Task<AccountLedgerViewModel> GetAccountLedger(int accountId, DateTime? fromDate, DateTime? toDate)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) throw new Exception("Account not found");

            var lines = await _context.JournalEntryLines
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == accountId)
                .OrderBy(l => l.JournalEntry!.Date)
                .ToListAsync();

            decimal runningBalance = 0;
            var ledgerLines = new List<AccountLedgerLine>();

            foreach (var line in lines)
            {
                decimal amount = (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
                    ? line.Debit - line.Credit
                    : line.Credit - line.Debit;

                runningBalance += amount;

                if ((!fromDate.HasValue || line.JournalEntry!.Date >= fromDate.Value) &&
                    (!toDate.HasValue || line.JournalEntry!.Date <= toDate.Value))
                {
                    ledgerLines.Add(new AccountLedgerLine
                    {
                        Date = line.JournalEntry!.Date,
                        Description = line.JournalEntry.Description,
                        Debit = line.Debit,
                        Credit = line.Credit,
                        RunningBalance = runningBalance
                    });
                }
            }

            return new AccountLedgerViewModel
            {
                AccountId = accountId,
                AccountName = account.Name,
                AccountCode = account.Code,
                Lines = ledgerLines,
                ClosingBalance = runningBalance
            };
        }
    }
}

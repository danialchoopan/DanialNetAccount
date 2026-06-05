namespace DanialNetAccount.ViewModels
{
    public class TrialBalanceReport
    {
        public List<TrialBalanceLine> Lines { get; set; } = new List<TrialBalanceLine>();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }

    public class TrialBalanceLine
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance => Debit - Credit;
    }

    public class ProfitLossReport
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit => TotalRevenue - TotalExpenses;
        public List<ProfitLossLine> RevenueLines { get; set; } = new List<ProfitLossLine>();
        public List<ProfitLossLine> ExpenseLines { get; set; } = new List<ProfitLossLine>();
    }

    public class ProfitLossLine
    {
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class BalanceSheetReport
    {
        public List<BalanceSheetLine> AssetLines { get; set; } = new List<BalanceSheetLine>();
        public List<BalanceSheetLine> LiabilityLines { get; set; } = new List<BalanceSheetLine>();
        public List<BalanceSheetLine> EquityLines { get; set; } = new List<BalanceSheetLine>();
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    }

    public class BalanceSheetLine
    {
        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }

    public class AccountLedgerViewModel
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public List<AccountLedgerLine> Lines { get; set; } = new List<AccountLedgerLine>();
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
    }

    public class AccountLedgerLine
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }
}

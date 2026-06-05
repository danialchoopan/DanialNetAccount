namespace DanialNetAccount.Models.Enums
{
    public enum AccountType
    {
        Asset,
        Liability,
        Equity,
        Revenue,
        Expense
    }

    public enum InventoryValuationMethod
    {
        FIFO,
        Average
    }

    public enum TransactionType
    {
        Purchase,
        Sale
    }
}

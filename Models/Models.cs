using System.ComponentModel.DataAnnotations;
using DanialNetAccount.Models.Enums;

namespace DanialNetAccount.Models
{
    public enum StoreType { Retail, Wholesale, ServiceBased, Manufacturing }
    public enum AppLanguage { English, Persian }
    public enum CurrencyType { USD, Toman }
    public enum InvoiceType { Sale, Purchase }
    public enum InvoiceStatus { Active, Voided }

    public class GlobalSetting
    {
        public int Id { get; set; }
        public InventoryValuationMethod ValuationMethod { get; set; } = InventoryValuationMethod.FIFO;
        public DateTime? ClosedUntilDate { get; set; }
        public StoreType StoreType { get; set; } = StoreType.Retail;
        public string CompanyName { get; set; } = "DanialNet Account";
        public AppLanguage Language { get; set; } = AppLanguage.English;
        public CurrencyType Currency { get; set; } = CurrencyType.USD;
    }

    public class Account
    {
        public int Id { get; set; }
        [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
        [Required, StringLength(20)] public string Code { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public int? ParentAccountId { get; set; }
        public Account? ParentAccount { get; set; }
        public ICollection<Account> Children { get; set; } = new List<Account>();
    }

    public class JournalEntry
    {
        public int Id { get; set; }
        [Required] public DateTime Date { get; set; }
        [Required] public string Description { get; set; } = string.Empty;
        public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine
    {
        public int Id { get; set; }
        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = "General";
    }

    public class InventoryTransaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
        public int RemainingQuantity { get; set; }
        public int? InvoiceId { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Active;
        public InvoiceType Type { get; set; } = InvoiceType.Sale;

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

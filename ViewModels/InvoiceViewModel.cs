using DanialNetAccount.Models;

namespace DanialNetAccount.ViewModels
{
    public class InvoiceViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public InvoiceType Type { get; set; } = InvoiceType.Sale;
        public List<InvoiceItemViewModel> Items { get; set; } = new List<InvoiceItemViewModel>();
    }

    public class InvoiceItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

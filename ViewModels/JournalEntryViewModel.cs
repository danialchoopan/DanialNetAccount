using DanialNetAccount.Models;

namespace DanialNetAccount.ViewModels
{
    public class JournalEntryViewModel
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public List<JournalEntryLineViewModel> Lines { get; set; } = new List<JournalEntryLineViewModel>();
    }

    public class JournalEntryLineViewModel
    {
        public int AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}

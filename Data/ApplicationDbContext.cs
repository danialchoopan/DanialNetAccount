using Microsoft.EntityFrameworkCore;
using DanialNetAccount.Models;

namespace DanialNetAccount.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<GlobalSetting> Settings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.ParentAccount)
                .WithMany(a => a.Children)
                .HasForeignKey(a => a.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(l => l.JournalEntry)
                .WithMany(e => e.Lines)
                .HasForeignKey(l => l.JournalEntryId);

            modelBuilder.Entity<JournalEntryLine>()
                .HasOne(l => l.Account)
                .WithMany()
                .HasForeignKey(l => l.AccountId);
        }
    }
}

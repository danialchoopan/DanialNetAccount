using DanialNetAccount.Models;
using DanialNetAccount.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DanialNetAccount.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Accounts.Any())
            {
                return;   // DB has been seeded
            }

            var assets = new Account { Name = "Assets", Code = "1", Type = AccountType.Asset };
            var liabilities = new Account { Name = "Liabilities", Code = "2", Type = AccountType.Liability };
            var equity = new Account { Name = "Equity", Code = "3", Type = AccountType.Equity };
            var revenue = new Account { Name = "Revenue", Code = "4", Type = AccountType.Revenue };
            var expenses = new Account { Name = "Expenses", Code = "5", Type = AccountType.Expense };

            context.Accounts.AddRange(assets, liabilities, equity, revenue, expenses);
            context.SaveChanges();

            var currentAssets = new Account { Name = "Current Assets", Code = "11", Type = AccountType.Asset, ParentAccountId = assets.Id };
            var bank = new Account { Name = "Bank", Code = "111", Type = AccountType.Asset, ParentAccountId = currentAssets.Id };

            var sales = new Account { Name = "Sales Revenue", Code = "41", Type = AccountType.Revenue, ParentAccountId = revenue.Id };

            context.Accounts.AddRange(currentAssets, bank, sales);
            context.SaveChanges();

            var product = new Product { Name = "Laptop", Price = 1200 };
            context.Products.Add(product);
            context.SaveChanges();

            if (!context.Settings.Any())
            {
                context.Settings.Add(new GlobalSetting { ValuationMethod = InventoryValuationMethod.FIFO });
                context.SaveChanges();
            }
        }
    }
}

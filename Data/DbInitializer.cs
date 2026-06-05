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

            if (context.Accounts.Any()) return;

            // 1. Root Accounts
            var assets = new Account { Name = "Assets", Code = "1", Type = AccountType.Asset };
            var liabilities = new Account { Name = "Liabilities", Code = "2", Type = AccountType.Liability };
            var equity = new Account { Name = "Equity", Code = "3", Type = AccountType.Equity };
            var revenue = new Account { Name = "Revenue", Code = "4", Type = AccountType.Revenue };
            var expenses = new Account { Name = "Expenses", Code = "5", Type = AccountType.Expense };

            context.Accounts.AddRange(assets, liabilities, equity, revenue, expenses);
            context.SaveChanges();

            // 2. Sub Accounts
            var bank = new Account { Name = "Main Bank Account", Code = "111", Type = AccountType.Asset, ParentAccountId = assets.Id };
            var inventory = new Account { Name = "Inventory", Code = "121", Type = AccountType.Asset, ParentAccountId = assets.Id };
            var ar = new Account { Name = "Accounts Receivable", Code = "131", Type = AccountType.Asset, ParentAccountId = assets.Id };

            var ap = new Account { Name = "Accounts Payable", Code = "211", Type = AccountType.Liability, ParentAccountId = liabilities.Id };
            var taxPayable = new Account { Name = "Sales Tax Payable", Code = "221", Type = AccountType.Liability, ParentAccountId = liabilities.Id };

            var capital = new Account { Name = "Owner Capital", Code = "311", Type = AccountType.Equity, ParentAccountId = equity.Id };
            var retainedEarnings = new Account { Name = "Retained Earnings", Code = "321", Type = AccountType.Equity, ParentAccountId = equity.Id };

            var sales = new Account { Name = "Sales Revenue", Code = "411", Type = AccountType.Revenue, ParentAccountId = revenue.Id };

            var cogs = new Account { Name = "Cost of Goods Sold", Code = "511", Type = AccountType.Expense, ParentAccountId = expenses.Id };
            var rent = new Account { Name = "Rent Expense", Code = "521", Type = AccountType.Expense, ParentAccountId = expenses.Id };
            var utilities = new Account { Name = "Utilities Expense", Code = "531", Type = AccountType.Expense, ParentAccountId = expenses.Id };

            context.Accounts.AddRange(bank, inventory, ar, ap, taxPayable, capital, retainedEarnings, sales, cogs, rent, utilities);
            context.SaveChanges();

            // 3. Products
            var products = new List<Product>
            {
                new Product { Name = "Pro Laptop X1", Price = 1500, Category = "Electronics" },
                new Product { Name = "Smartphone Ultra", Price = 900, Category = "Electronics" },
                new Product { Name = "Wireless Headphones", Price = 200, Category = "Accessories" },
                new Product { Name = "Office Chair", Price = 350, Category = "Furniture" },
                new Product { Name = "Mechanical Keyboard", Price = 120, Category = "Accessories" }
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            // 4. Initial Transactions (Purchases & Capital)
            var capitalEntry = new JournalEntry
            {
                Date = DateTime.Now.AddDays(-30),
                Description = "Initial Investment",
                Lines = new List<JournalEntryLine>
                {
                    new JournalEntryLine { AccountId = bank.Id, Debit = 50000, Credit = 0 },
                    new JournalEntryLine { AccountId = capital.Id, Debit = 0, Credit = 50000 }
                }
            };
            context.JournalEntries.Add(capitalEntry);

            // Purchase Stock
            foreach (var p in products)
            {
                decimal purchasePrice = p.Price * 0.7m;
                int qty = 50;
                context.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = p.Id,
                    Quantity = qty,
                    RemainingQuantity = qty,
                    UnitPrice = purchasePrice,
                    Date = DateTime.Now.AddDays(-25),
                    Type = TransactionType.Purchase
                });

                context.JournalEntries.Add(new JournalEntry
                {
                    Date = DateTime.Now.AddDays(-25),
                    Description = $"Purchase of {p.Name}",
                    Lines = new List<JournalEntryLine>
                    {
                        new JournalEntryLine { AccountId = inventory.Id, Debit = purchasePrice * qty, Credit = 0 },
                        new JournalEntryLine { AccountId = bank.Id, Debit = 0, Credit = purchasePrice * qty }
                    }
                });
            }
            context.SaveChanges();

            // 5. Sample Invoices
            var customers = new[] { "Alpha Corp", "Beta LLC", "Gamma Inc", "Individual Client" };
            for(int i = 0; i < 10; i++)
            {
                var invDate = DateTime.Now.AddDays(-20 + i);
                var inv = new Invoice
                {
                    InvoiceNumber = "INV-2025-" + (1001 + i),
                    CustomerName = customers[i % customers.Length],
                    Date = invDate,
                    Status = InvoiceStatus.Active
                };

                var prod = products[i % products.Count];
                int qty = 2 + (i % 3);

                inv.Items.Add(new InvoiceItem
                {
                    ProductId = prod.Id,
                    Quantity = qty,
                    UnitPrice = prod.Price,
                    TotalPrice = qty * prod.Price
                });

                inv.TotalAmount = inv.Items.Sum(x => x.TotalPrice);
                inv.TaxAmount = inv.TotalAmount * 0.09m;
                inv.FinalAmount = inv.TotalAmount + inv.TaxAmount;

                context.Invoices.Add(inv);

                // Inventory deduction with batch update
                decimal purchasePrice = prod.Price * 0.7m;
                decimal totalCOGS = qty * purchasePrice;

                var batch = context.InventoryTransactions
                    .Where(t => t.ProductId == prod.Id && t.Type == TransactionType.Purchase && t.RemainingQuantity >= qty)
                    .FirstOrDefault();

                if (batch != null) batch.RemainingQuantity -= qty;

                context.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductId = prod.Id,
                    Quantity = qty,
                    RemainingQuantity = 0,
                    UnitPrice = purchasePrice,
                    Date = invDate,
                    Type = TransactionType.Sale
                });

                // Ledger entries for invoice
                context.JournalEntries.Add(new JournalEntry
                {
                    Date = invDate,
                    Description = $"Invoice {inv.InvoiceNumber} - {inv.CustomerName}",
                    Lines = new List<JournalEntryLine>
                    {
                        new JournalEntryLine { AccountId = bank.Id, Debit = inv.FinalAmount, Credit = 0 },
                        new JournalEntryLine { AccountId = sales.Id, Debit = 0, Credit = inv.TotalAmount },
                        new JournalEntryLine { AccountId = taxPayable.Id, Debit = 0, Credit = inv.TaxAmount },
                        new JournalEntryLine { AccountId = cogs.Id, Debit = totalCOGS, Credit = 0 },
                        new JournalEntryLine { AccountId = inventory.Id, Debit = 0, Credit = totalCOGS }
                    }
                });
            }

            // 6. Global Settings
            context.Settings.Add(new GlobalSetting
            {
                ValuationMethod = InventoryValuationMethod.FIFO,
                CompanyName = "DanialNet Tech Solutions",
                StoreType = StoreType.Retail
            });
            context.SaveChanges();
        }
    }
}

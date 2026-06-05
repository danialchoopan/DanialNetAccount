using DanialNetAccount.Data;
using DanialNetAccount.Models;
using DanialNetAccount.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DanialNetAccount.Services.Inventory
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task PurchaseProduct(int productId, int quantity, decimal unitPrice)
        {
            var transaction = new InventoryTransaction
            {
                ProductId = productId,
                Quantity = quantity,
                RemainingQuantity = quantity,
                UnitPrice = unitPrice,
                Date = DateTime.Now,
                Type = TransactionType.Purchase
            };

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> SellProduct(int productId, int quantity, IInventoryValuationStrategy strategy, int? invoiceId = null)
        {
            var transactions = await _context.InventoryTransactions
                .Where(t => t.ProductId == productId)
                .ToListAsync();

            decimal costOfGoodsSold = strategy.CalculateCostOfGoodsSold(transactions, quantity);

            var saleTransaction = new InventoryTransaction
            {
                ProductId = productId,
                Quantity = quantity,
                RemainingQuantity = 0,
                UnitPrice = costOfGoodsSold / quantity,
                Date = DateTime.Now,
                Type = TransactionType.Sale,
                InvoiceId = invoiceId
            };

            _context.InventoryTransactions.Add(saleTransaction);
            await _context.SaveChangesAsync();

            return costOfGoodsSold;
        }

        public async Task RestoreProduct(int invoiceId)
        {
            var sales = await _context.InventoryTransactions
                .Where(t => t.InvoiceId == invoiceId && t.Type == TransactionType.Sale)
                .ToListAsync();

            foreach (var sale in sales)
            {
                // Restore quantities to purchases for this product
                int remainingToRestore = sale.Quantity;
                var purchases = await _context.InventoryTransactions
                    .Where(t => t.ProductId == sale.ProductId && t.Type == TransactionType.Purchase)
                    .OrderByDescending(t => t.Date) // Restoring to the ones that would have been used last if we want to be exact with FIFO reversal, or just any. Actually, FIFO used the oldest first, so reversal should restore to those.
                    .ToListAsync();

                // For FIFO reversal, we should ideally know which ones were deducted.
                // But since we just subtract from RemainingQuantity, we can just add back to any that have space or the latest ones.
                // A better way is to track the deductions in a separate table, but for this demo we'll just restore to the first available purchase.
                foreach(var p in purchases.OrderBy(p => p.Date))
                {
                    if (remainingToRestore <= 0) break;
                    int canRestore = p.Quantity - p.RemainingQuantity;
                    int restore = Math.Min(canRestore, remainingToRestore);
                    p.RemainingQuantity += restore;
                    remainingToRestore -= restore;
                }

                _context.InventoryTransactions.Remove(sale);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetStockLevel(int productId)
        {
            return await _context.InventoryTransactions
                .Where(t => t.ProductId == productId && t.Type == TransactionType.Purchase)
                .SumAsync(t => t.RemainingQuantity);
        }
    }
}

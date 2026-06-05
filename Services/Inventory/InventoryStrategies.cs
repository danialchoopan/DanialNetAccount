using DanialNetAccount.Models;

namespace DanialNetAccount.Services.Inventory
{
    public interface IInventoryValuationStrategy
    {
        decimal CalculateCostOfGoodsSold(List<InventoryTransaction> transactions, int quantityToSell);
    }

    public class FIFOValuationStrategy : IInventoryValuationStrategy
    {
        public decimal CalculateCostOfGoodsSold(List<InventoryTransaction> transactions, int quantityToSell)
        {
            decimal totalCost = 0;
            int remainingToSell = quantityToSell;

            var purchases = transactions
                .Where(t => t.Type == Models.Enums.TransactionType.Purchase && t.RemainingQuantity > 0)
                .OrderBy(t => t.Date)
                .ToList();

            foreach (var purchase in purchases)
            {
                if (remainingToSell <= 0) break;

                int take = Math.Min(purchase.RemainingQuantity, remainingToSell);
                totalCost += take * purchase.UnitPrice;
                purchase.RemainingQuantity -= take;
                remainingToSell -= take;
            }

            if (remainingToSell > 0)
            {
                // This shouldn't happen if inventory is checked before selling
                throw new InvalidOperationException("Not enough inventory to fulfill the sale.");
            }

            return totalCost;
        }
    }

    public class AverageValuationStrategy : IInventoryValuationStrategy
    {
        public decimal CalculateCostOfGoodsSold(List<InventoryTransaction> transactions, int quantityToSell)
        {
            int totalQuantity = transactions
                .Where(t => t.Type == Models.Enums.TransactionType.Purchase)
                .Sum(t => t.RemainingQuantity);

            if (totalQuantity < quantityToSell)
                throw new InvalidOperationException("Not enough inventory.");

            decimal totalValue = transactions
                .Where(t => t.Type == Models.Enums.TransactionType.Purchase)
                .Sum(t => t.RemainingQuantity * t.UnitPrice);

            decimal averageCost = totalValue / totalQuantity;

            // For simplicity in this demo, we just update remaining quantities
            int remainingToSell = quantityToSell;
            var purchases = transactions
                .Where(t => t.Type == Models.Enums.TransactionType.Purchase && t.RemainingQuantity > 0)
                .OrderBy(t => t.Date)
                .ToList();

            foreach (var purchase in purchases)
            {
                if (remainingToSell <= 0) break;
                int take = Math.Min(purchase.RemainingQuantity, remainingToSell);
                purchase.RemainingQuantity -= take;
                remainingToSell -= take;
            }

            return quantityToSell * averageCost;
        }
    }
}

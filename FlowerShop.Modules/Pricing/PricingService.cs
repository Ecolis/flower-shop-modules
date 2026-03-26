using FlowerShop.Modules.Inventory;

namespace FlowerShop.Modules.Pricing;

/// <summary>
/// Реализация сервиса ценообразования
/// </summary>
public class PricingService : IPricingService
{
    private readonly IInventoryService _inventoryService;
    private readonly Dictionary<string, decimal> _prices = new();

    public PricingService(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;

        // Инициализация ценами
        _prices["rose"] = 150m;
        _prices["tulip"] = 80m;
        _prices["chamomile"] = 60m;
    }

    public decimal GetPrice(string flowerId)
    {
        return _prices.GetValueOrDefault(flowerId, 0);
    }

    public decimal CalculateOrderPrice(Dictionary<string, int> items)
    {
        decimal total = 0;

        foreach (var (flowerId, quantity) in items)
        {
            // Проверяем наличие через сервис склада
            if (!_inventoryService.CheckAvailability(flowerId, quantity))
            {
                var flower = _inventoryService.GetFlower(flowerId);
                throw new InvalidOperationException(
                    $"Товар {flower?.Name ?? flowerId} недоступен в количестве {quantity}");
            }

            var price = GetPrice(flowerId);
            total += price * quantity;

            Console.WriteLine($"[Pricing] {flowerId} x{quantity} = {price * quantity} руб.");
        }

        return total;
    }
}
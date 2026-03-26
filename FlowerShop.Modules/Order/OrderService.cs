using FlowerShop.Modules.Inventory;
using FlowerShop.Modules.Pricing;

namespace FlowerShop.Modules.Order;

/// <summary>
/// Реализация сервиса оформления заказов
/// </summary>
public class OrderService : IOrderService
{
    private readonly IInventoryService _inventoryService;
    private readonly IPricingService _pricingService;

    public OrderService(IInventoryService inventoryService, IPricingService pricingService)
    {
        _inventoryService = inventoryService;
        _pricingService = pricingService;
    }

    public async Task<Receipt> CreateOrderAsync(Dictionary<string, int> items)
    {
        Console.WriteLine("\n[OrderService] Начинаем оформление заказа...");

        // 1. Проверяем наличие всех товаров
        foreach (var (flowerId, quantity) in items)
        {
            if (!_inventoryService.CheckAvailability(flowerId, quantity))
            {
                var flower = _inventoryService.GetFlower(flowerId);
                throw new InvalidOperationException(
                    $"Товар {flower?.Name ?? flowerId} отсутствует в нужном количестве. Запрошено: {quantity}");
            }
        }
        Console.WriteLine("[OrderService] Наличие подтверждено");

        // 2. Рассчитываем стоимость
        decimal totalPrice = _pricingService.CalculateOrderPrice(items);
        Console.WriteLine($"[OrderService] Итоговая стоимость: {totalPrice} руб.");

        // 3. Резервируем товары (уменьшаем остатки)
        foreach (var (flowerId, quantity) in items)
        {
            _inventoryService.Reserve(flowerId, quantity);
        }

        // 4. Формируем чек
        var receiptItems = new List<OrderItem>();
        foreach (var (flowerId, quantity) in items)
        {
            var flower = _inventoryService.GetFlower(flowerId);
            var unitPrice = _pricingService.GetPrice(flowerId);

            receiptItems.Add(new OrderItem
            {
                FlowerId = flowerId,
                FlowerName = flower?.Name ?? flowerId,
                Quantity = quantity,
                UnitPrice = unitPrice
            });
        }

        var receipt = new Receipt
        {
            OrderId = Guid.NewGuid().ToString(),
            Items = receiptItems,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.Now
        };

        Console.WriteLine("[OrderService] Заказ успешно оформлен!");

        return receipt;
    }
}
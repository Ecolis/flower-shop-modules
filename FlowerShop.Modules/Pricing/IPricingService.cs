namespace FlowerShop.Modules.Pricing;

/// <summary>
/// Сервис ценообразования
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Получить цену цветка
    /// </summary>
    decimal GetPrice(string flowerId);

    /// <summary>
    /// Рассчитать стоимость заказа
    /// </summary>
    /// <param name="items">Словарь: flowerId -> quantity</param>
    decimal CalculateOrderPrice(Dictionary<string, int> items);
}
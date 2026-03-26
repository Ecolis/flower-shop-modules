namespace FlowerShop.Modules.Order;

/// <summary>
/// Сервис оформления заказов
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Оформить заказ
    /// </summary>
    /// <param name="items">Словарь: flowerId -> quantity</param>
    /// <returns>Чек с деталями заказа</returns>
    Task<Receipt> CreateOrderAsync(Dictionary<string, int> items);
}
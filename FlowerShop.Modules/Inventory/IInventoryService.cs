namespace FlowerShop.Modules.Inventory;

/// <summary>
/// Сервис учета товаров (склад)
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Проверить наличие товара в нужном количестве
    /// </summary>
    bool CheckAvailability(string flowerId, int quantity);

    /// <summary>
    /// Зарезервировать товар (уменьшить остаток)
    /// </summary>
    bool Reserve(string flowerId, int quantity);

    /// <summary>
    /// Получить текущие остатки
    /// </summary>
    IReadOnlyDictionary<string, int> GetStockLevels();

    /// <summary>
    /// Получить информацию о цветке по ID
    /// </summary>
    Flower? GetFlower(string flowerId);
}
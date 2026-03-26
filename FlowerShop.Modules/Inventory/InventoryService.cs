namespace FlowerShop.Modules.Inventory;

/// <summary>
/// Реализация сервиса учета товаров
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly Dictionary<string, StockItem> _stock = new();
    private readonly Dictionary<string, Flower> _flowers = new();

    public InventoryService()
    {
        // Инициализация тестовыми данными
        LoadTestData();
    }

    private void LoadTestData()
    {
        var flowers = new[]
        {
            new Flower { Id = "rose", Name = "Роза" },
            new Flower { Id = "tulip", Name = "Тюльпан" },
            new Flower { Id = "chamomile", Name = "Ромашка" }
        };

        foreach (var flower in flowers)
        {
            _flowers[flower.Id] = flower;
            var quantity = flower.Id switch
            {
                "rose" => 50,
                "tulip" => 30,
                "chamomile" => 40,
                _ => 0
            };
            _stock[flower.Id] = new StockItem { Flower = flower, Quantity = quantity };
        }
    }

    public bool CheckAvailability(string flowerId, int quantity)
    {
        if (!_stock.TryGetValue(flowerId, out var item))
            return false;

        return item.Quantity >= quantity;
    }

    public bool Reserve(string flowerId, int quantity)
    {
        if (!CheckAvailability(flowerId, quantity))
            return false;

        _stock[flowerId].Quantity -= quantity;
        Console.WriteLine($"[Inventory] Зарезервировано {quantity} шт. {_flowers[flowerId].Name}. Остаток: {_stock[flowerId].Quantity}");
        return true;
    }

    public IReadOnlyDictionary<string, int> GetStockLevels()
    {
        return _stock.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Quantity);
    }

    public Flower? GetFlower(string flowerId)
    {
        return _flowers.GetValueOrDefault(flowerId);
    }
}

namespace FlowerShop.Modules.Inventory;

/// <summary>
/// Модель цветка (товара)
/// </summary>
public record Flower
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// Модель остатка на складе
/// </summary>
public record StockItem
{
    public Flower Flower { get; init; } = null!;
    public int Quantity { get; set; }
}
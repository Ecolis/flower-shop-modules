namespace FlowerShop.Modules.Order;

/// <summary>
/// Позиция заказа
/// </summary>
public record OrderItem
{
    public string FlowerId { get; init; } = string.Empty;
    public string FlowerName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

/// <summary>
/// Заказ
/// </summary>
public record Order
{
    public string OrderId { get; init; } = Guid.NewGuid().ToString();
    public List<OrderItem> Items { get; init; } = new();
    public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
    public DateTime CreatedAt { get; init; } = DateTime.Now;
}

/// <summary>
/// Чек (результат оформления заказа)
/// </summary>
public record Receipt
{
    public string OrderId { get; init; } = string.Empty;
    public List<OrderItem> Items { get; init; } = new();
    public decimal TotalPrice { get; init; }
    public DateTime CreatedAt { get; init; }

    public override string ToString()
    {
        var itemsText = string.Join("\n", Items.Select(i =>
            $"  {i.FlowerName} x{i.Quantity} = {i.TotalPrice} руб."));

        return $@"
=== ЧЕК ===
Номер заказа: {OrderId}
Дата: {CreatedAt:dd.MM.yyyy HH:mm}
Товары:
{itemsText}
------------------------
ИТОГО: {TotalPrice} руб.
===========";
    }
}
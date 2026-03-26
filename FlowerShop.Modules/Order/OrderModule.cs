using FlowerShop.Core;
using FlowerShop.Modules.Inventory;
using FlowerShop.Modules.Pricing;
using Microsoft.Extensions.DependencyInjection;

namespace FlowerShop.Modules.Order;

/// <summary>
/// Модуль оформления заказов
/// Зависит от InventoryModule и PricingModule
/// </summary>
public class OrderModule : IModule
{
    public string Name => "Order";

    public IReadOnlyList<string> Dependencies => new[] { "Inventory", "Pricing" };

    public void RegisterServices(IServiceCollection services)
    {
        Console.WriteLine("[OrderModule] Регистрация сервисов...");

        // Регистрируем сервис оформления заказов
        services.AddScoped<IOrderService, OrderService>();
    }

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("[OrderModule] Инициализация...");

        // Проверяем, что зависимости доступны
        var inventoryService = serviceProvider.GetService<IInventoryService>();
        var pricingService = serviceProvider.GetService<IPricingService>();

        if (inventoryService == null)
            Console.WriteLine("[OrderModule] ВНИМАНИЕ: IInventoryService не найден!");
        if (pricingService == null)
            Console.WriteLine("[OrderModule] ВНИМАНИЕ: IPricingService не найден!");

        if (inventoryService != null && pricingService != null)
        {
            Console.WriteLine("[OrderModule] Все зависимости успешно загружены");
        }

        await Task.CompletedTask;
    }
}
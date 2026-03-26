using FlowerShop.Core;
using FlowerShop.Modules.Inventory;
using Microsoft.Extensions.DependencyInjection;

namespace FlowerShop.Modules.Pricing;

/// <summary>
/// Модуль ценообразования
/// Зависит от InventoryModule (нужен для проверки наличия)
/// </summary>
public class PricingModule : IModule
{
    public string Name => "Pricing";

    public IReadOnlyList<string> Dependencies => new[] { "Inventory" };

    public void RegisterServices(IServiceCollection services)
    {
        Console.WriteLine("[PricingModule] Регистрация сервисов...");

        // Регистрируем сервис ценообразования
        services.AddSingleton<IPricingService, PricingService>();
    }

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("[PricingModule] Инициализация...");

        // Проверяем, что зависимость от Inventory доступна
        var inventoryService = serviceProvider.GetService<IInventoryService>();
        if (inventoryService == null)
        {
            Console.WriteLine("[PricingModule] ВНИМАНИЕ: IInventoryService не найден!");
        }
        else
        {
            var pricingService = serviceProvider.GetRequiredService<IPricingService>();
            Console.WriteLine("[PricingModule] Текущие цены:");
            foreach (var flowerId in new[] { "rose", "tulip", "chamomile" })
            {
                var flower = inventoryService.GetFlower(flowerId);
                var price = pricingService.GetPrice(flowerId);
                Console.WriteLine($"  - {flower?.Name}: {price} руб.");
            }
        }

        await Task.CompletedTask;
    }
}
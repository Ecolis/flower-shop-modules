using Microsoft.Extensions.DependencyInjection;
using FlowerShop.Core;

namespace FlowerShop.Modules.Inventory;

/// <summary>
/// Модуль учета товаров (склад)
/// Не имеет зависимостей от других модулей
/// </summary>
public class InventoryModule : IModule
{
    public string Name => "Inventory";

    public IReadOnlyList<string> Dependencies => Array.Empty<string>();

    public void RegisterServices(IServiceCollection services)
    {
        Console.WriteLine("[InventoryModule] Регистрация сервисов...");

        // Регистрируем сервис учета товаров как Singleton
        // (склад один на всё приложение)
        services.AddSingleton<IInventoryService, InventoryService>();
    }

    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("[InventoryModule] Инициализация...");

        // Получаем сервис для проверки, что он зарегистрирован
        var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();

        // Выводим начальные остатки
        Console.WriteLine("[InventoryModule] Начальные остатки на складе:");
        foreach (var (id, quantity) in inventoryService.GetStockLevels())
        {
            var flower = inventoryService.GetFlower(id);
            Console.WriteLine($"  - {flower?.Name}: {quantity} шт.");
        }

        await Task.CompletedTask;
    }
}
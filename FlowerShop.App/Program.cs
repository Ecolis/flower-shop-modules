using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FlowerShop.Core;
using FlowerShop.Modules.Inventory;
using FlowerShop.Modules.Pricing;
using FlowerShop.Modules.Order;

namespace FlowerShop.App;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Цветочный магазин ===\n");

        // 1. Загружаем конфигурацию
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)  // ← изменено
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Используем GetSection и Value для получения массива
        var moduleNames = configuration.GetSection("Modules").Get<string[]>();
        if (moduleNames == null || moduleNames.Length == 0)
        {
            Console.WriteLine("Ошибка: в файле appsettings.json не указаны модули");
            return;
        }

        Console.WriteLine("=== Этап 1: Чтение конфигурации ===");
        Console.WriteLine($"Модули из файла appsettings.json: {string.Join(", ", moduleNames)}");

        // 2. Создаем фабрику модулей и регистрируем доступные модули
        var factory = new ModuleFactory();
        factory.Register("Inventory", () => new InventoryModule());
        factory.Register("Pricing", () => new PricingModule());
        factory.Register("Order", () => new OrderModule());

        // 3. Загружаем модули через фабрику
        var modules = new List<IModule>();
        foreach (var name in moduleNames)
        {
            try
            {
                var module = factory.Create(name);
                modules.Add(module);
                Console.WriteLine($"  Загружен модуль: {name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Ошибка загрузки модуля '{name}': {ex.Message}");
                return;
            }
        }

        // 4. Проверяем зависимости и строим порядок запуска
        Console.WriteLine("\n=== Этап 2: Проверка зависимостей и сортировка ===");
        try
        {
            var executionOrder = GetExecutionOrder(modules);
            Console.WriteLine("Порядок запуска модулей:");
            for (int i = 0; i < executionOrder.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {executionOrder[i].Name}");
            }

            // 5. Регистрируем сервисы
            var services = new ServiceCollection();
            Console.WriteLine("\n=== Этап 3: Регистрация сервисов ===");
            foreach (var module in executionOrder)
            {
                module.RegisterServices(services);
            }

            // 6. Строим провайдер
            var serviceProvider = services.BuildServiceProvider();

            // 7. Инициализируем модули
            Console.WriteLine("\n=== Этап 4: Инициализация модулей ===");
            foreach (var module in executionOrder)
            {
                await module.InitializeAsync(serviceProvider);
            }

            // 8. Демонстрация работы
            Console.WriteLine("\n=== Этап 5: Работа магазина ===");
            await DemonstrateStoreWork(serviceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nОШИБКА: {ex.Message}");
            return;
        }

        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    /// <summary>
    /// Топологическая сортировка модулей по зависимостям
    /// </summary>
    static List<IModule> GetExecutionOrder(List<IModule> modules)
    {
        var moduleDict = modules.ToDictionary(m => m.Name, m => m);
        var result = new List<IModule>();
        var visited = new HashSet<string>();
        var inStack = new HashSet<string>();

        void Visit(IModule module)
        {
            if (visited.Contains(module.Name))
                return;

            if (inStack.Contains(module.Name))
            {
                throw new InvalidOperationException(
                    $"Обнаружена циклическая зависимость! Модуль '{module.Name}' участвует в цикле.");
            }

            inStack.Add(module.Name);

            // Сначала обрабатываем зависимости
            foreach (var depName in module.Dependencies)
            {
                if (!moduleDict.ContainsKey(depName))
                {
                    throw new InvalidOperationException(
                        $"Модуль '{module.Name}' требует модуль '{depName}', который не найден. " +
                        $"Доступные модули: {string.Join(", ", moduleDict.Keys)}");
                }

                Visit(moduleDict[depName]);
            }

            inStack.Remove(module.Name);
            visited.Add(module.Name);
            result.Add(module);
        }

        foreach (var module in modules)
        {
            Visit(module);
        }

        return result;
    }

    /// <summary>
    /// Демонстрация работы магазина
    /// </summary>
    static async Task DemonstrateStoreWork(IServiceProvider serviceProvider)
    {
        // Получаем сервисы через DI
        var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
        var pricingService = serviceProvider.GetRequiredService<IPricingService>();
        var orderService = serviceProvider.GetRequiredService<IOrderService>();

        // Показываем текущие остатки
        Console.WriteLine("\nТекущие остатки на складе:");
        foreach (var (id, quantity) in inventoryService.GetStockLevels())
        {
            var flower = inventoryService.GetFlower(id);
            var price = pricingService.GetPrice(id);
            Console.WriteLine($"  {flower?.Name}: {quantity} шт. (цена: {price} руб.)");
        }

        // Оформляем заказ
        Console.WriteLine("\n--- Оформление заказа ---");
        var orderItems = new Dictionary<string, int>
        {
            ["rose"] = 3,      // 3 розы
            ["tulip"] = 5,     // 5 тюльпанов
            ["chamomile"] = 2  // 2 ромашки
        };

        Console.WriteLine("Состав заказа:");
        foreach (var (id, quantity) in orderItems)
        {
            var flower = inventoryService.GetFlower(id);
            Console.WriteLine($"  {flower?.Name}: {quantity} шт.");
        }

        try
        {
            var receipt = await orderService.CreateOrderAsync(orderItems);
            Console.WriteLine(receipt.ToString());

            // Показываем обновленные остатки
            Console.WriteLine("\nОстатки после заказа:");
            foreach (var (id, quantity) in inventoryService.GetStockLevels())
            {
                var flower = inventoryService.GetFlower(id);
                Console.WriteLine($"  {flower?.Name}: {quantity} шт.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при оформлении заказа: {ex.Message}");
        }

        // Пробуем оформить заказ с недостаточным количеством
        Console.WriteLine("\n--- Попытка оформить заказ с недостаточным количеством ---");
        var badOrder = new Dictionary<string, int>
        {
            ["rose"] = 100  // роз всего 50, так что должно быть ошибка
        };

        try
        {
            var badReceipt = await orderService.CreateOrderAsync(badOrder);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ожидаемая ошибка: {ex.Message}");
        }
    }
}

/// <summary>
/// Фабрика для создания модулей по имени
/// </summary>
public interface IModuleFactory
{
    IModule Create(string moduleName);
}

/// <summary>
/// Реализация фабрики модулей
/// </summary>
public class ModuleFactory : IModuleFactory
{
    private readonly Dictionary<string, Func<IModule>> _creators = new();

    public void Register(string moduleName, Func<IModule> creator)
    {
        _creators[moduleName] = creator;
    }

    public IModule Create(string moduleName)
    {
        if (_creators.TryGetValue(moduleName, out var creator))
        {
            return creator();
        }

        throw new InvalidOperationException($"Модуль '{moduleName}' не зарегистрирован в фабрике");
    }
}
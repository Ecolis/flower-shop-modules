using FlowerShop.Core;

namespace FlowerShop.Tests;

/// <summary>
/// Тестовые модули для проверки сценариев
/// </summary>

// Модуль A без зависимостей
public class ModuleA : IModule
{
    public string Name => "A";
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();
    public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
    public Task InitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;
}

// Модуль B зависит от A
public class ModuleB : IModule
{
    public string Name => "B";
    public IReadOnlyList<string> Dependencies => new[] { "A" };
    public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
    public Task InitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;
}

// Модуль C зависит от B
public class ModuleC : IModule
{
    public string Name => "C";
    public IReadOnlyList<string> Dependencies => new[] { "B" };
    public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
    public Task InitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;
}

// Модуль X зависит от Y (Y отсутствует)
public class ModuleX : IModule
{
    public string Name => "X";
    public IReadOnlyList<string> Dependencies => new[] { "Y" };
    public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
    public Task InitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;
}

// Модуль Cycle1 зависит от Cycle2
public class Cycle1 : IModule
{
    public string Name => "Cycle1";
    public IReadOnlyList<string> Dependencies => new[] { "Cycle2" };
    public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
    public Task InitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;
}

// Модуль Cycle2 зависит от Cycle1 (образует цикл)
public class Cycle2 : IModule
{
    public string Name => "Cycle2";
    public IReadOnlyList<string> Dependencies => new[] { "Cycle1" };
    public void RegisterServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services) { }
    public Task InitializeAsync(IServiceProvider serviceProvider) => Task.CompletedTask;
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Тестирование системы модулей ===\n");

        // Тест 1: Корректный порядок запуска
        TestCorrectOrder();

        // Тест 2: Отсутствующий модуль
        TestMissingModule();

        // Тест 3: Циклические зависимости
        TestCircularDependency();

        Console.WriteLine("\nВсе тесты завершены. Нажмите любую клавишу для выхода...");
        Console.ReadKey();
    }

    /// <summary>
    /// Тест 1: Проверка корректного порядка запуска
    /// </summary>
    static void TestCorrectOrder()
    {
        Console.WriteLine("=== ТЕСТ 1: Корректный порядок запуска ===");

        var modules = new List<IModule>
        {
            new ModuleC(),
            new ModuleA(),
            new ModuleB()
        };

        Console.WriteLine("Исходный список модулей (в случайном порядке):");
        foreach (var m in modules)
        {
            Console.WriteLine($"  {m.Name} (зависит: {string.Join(",", m.Dependencies)})");
        }

        try
        {
            var order = GetExecutionOrder(modules);
            Console.WriteLine("\nПорядок запуска после сортировки:");
            for (int i = 0; i < order.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {order[i].Name}");
            }

            // Проверяем, что зависимости соблюдены
            bool isValid = true;
            var orderNames = order.Select(m => m.Name).ToList();

            // A должен быть перед B
            if (orderNames.IndexOf("A") > orderNames.IndexOf("B"))
            {
                Console.WriteLine("  ОШИБКА: A должен быть перед B");
                isValid = false;
            }

            // B должен быть перед C
            if (orderNames.IndexOf("B") > orderNames.IndexOf("C"))
            {
                Console.WriteLine("  ОШИБКА: B должен быть перед C");
                isValid = false;
            }

            if (isValid)
            {
                Console.WriteLine("\n✓ Результат: порядок запуска корректный");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Тест 2: Проверка ошибки отсутствующего модуля
    /// </summary>
    static void TestMissingModule()
    {
        Console.WriteLine("=== ТЕСТ 2: Отсутствующий модуль ===");

        var modules = new List<IModule>
        {
            new ModuleX()  // зависит от Y, которого нет
        };

        Console.WriteLine("Модуль X требует модуль Y, который отсутствует");

        try
        {
            var order = GetExecutionOrder(modules);
            Console.WriteLine("✗ Ошибка: система не обнаружила отсутствие модуля");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✓ Система обнаружила ошибку:");
            Console.WriteLine($"  Сообщение: {ex.Message}");

            // Проверяем, что сообщение понятное и содержит нужную информацию
            if (ex.Message.Contains("X") && ex.Message.Contains("Y"))
            {
                Console.WriteLine("  ✓ Сообщение содержит имена модулей");
            }
            else
            {
                Console.WriteLine("  ✗ Сообщение не содержит имена модулей");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Тест 3: Проверка ошибки циклических зависимостей
    /// </summary>
    static void TestCircularDependency()
    {
        Console.WriteLine("=== ТЕСТ 3: Циклические зависимости ===");

        var modules = new List<IModule>
        {
            new Cycle1(),
            new Cycle2()
        };

        Console.WriteLine("Модули с циклической зависимостью:");
        Console.WriteLine("  Cycle1 зависит от Cycle2");
        Console.WriteLine("  Cycle2 зависит от Cycle1");

        try
        {
            var order = GetExecutionOrder(modules);
            Console.WriteLine("✗ Ошибка: система не обнаружила циклическую зависимость");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✓ Система обнаружила ошибку:");
            Console.WriteLine($"  Сообщение: {ex.Message}");

            // Проверяем, что сообщение понятное
            if (ex.Message.Contains("цикл") || ex.Message.Contains("cycle") ||
                ex.Message.Contains("Cycle1") || ex.Message.Contains("Cycle2"))
            {
                Console.WriteLine("  ✓ Сообщение указывает на наличие цикла");
            }
            else
            {
                Console.WriteLine("  ✗ Сообщение не содержит информацию о цикле");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Топологическая сортировка модулей по зависимостям (та же, что в основном приложении)
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
}
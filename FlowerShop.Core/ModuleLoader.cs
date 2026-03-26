using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowerShop.Core;

/// <summary>
/// Загрузчик модулей. Отвечает за обнаружение, сортировку и инициализацию модулей.
/// </summary>
public class ModuleLoader
{
    private readonly IServiceCollection _services;
    private readonly List<IModule> _modules = new();

    public ModuleLoader(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Загрузить модули из указанных сборок
    /// </summary>
    /// <param name="assemblies">Сборки, в которых нужно искать модули</param>
    public void DiscoverModules(params Assembly[] assemblies)
    {
        var moduleTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in moduleTypes)
        {
            var module = (IModule)Activator.CreateInstance(type)!;
            _modules.Add(module);
            Console.WriteLine($"[ModuleLoader] Обнаружен модуль: {module.Name}");
        }
    }

    /// <summary>
    /// Получить порядок запуска модулей (топологическая сортировка)
    /// </summary>
    /// <returns>Модули в порядке, учитывающем зависимости</returns>
    /// <exception cref="InvalidOperationException">При циклических зависимостях или отсутствующих модулях</exception>
    public List<IModule> GetExecutionOrder()
    {
        // Строим словарь для быстрого доступа
        var moduleDict = _modules.ToDictionary(m => m.Name, m => m);

        // Проверяем, что все зависимости существуют
        foreach (var module in _modules)
        {
            foreach (var depName in module.Dependencies)
            {
                if (!moduleDict.ContainsKey(depName))
                {
                    throw new InvalidOperationException(
                        $"Модуль '{module.Name}' требует модуль '{depName}', который не найден. " +
                        $"Доступные модули: {string.Join(", ", moduleDict.Keys)}");
                }
            }
        }

        // Топологическая сортировка (DFS с отслеживанием состояния)
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
                    $"Обнаружена циклическая зависимость. Модуль '{module.Name}' участвует в цикле.");
            }

            inStack.Add(module.Name);

            // Сначала обрабатываем зависимости
            foreach (var depName in module.Dependencies)
            {
                var depModule = moduleDict[depName];
                Visit(depModule);
            }

            inStack.Remove(module.Name);
            visited.Add(module.Name);
            result.Add(module);
        }

        foreach (var module in _modules)
        {
            Visit(module);
        }

        return result;
    }

    /// <summary>
    /// Зарегистрировать все модули (вызвать RegisterServices)
    /// </summary>
    public void RegisterAllModules()
    {
        var order = GetExecutionOrder();

        foreach (var module in order)
        {
            Console.WriteLine($"[ModuleLoader] Регистрация сервисов модуля: {module.Name}");
            module.RegisterServices(_services);
        }
    }

    /// <summary>
    /// Инициализировать все модули в правильном порядке
    /// </summary>
    public async Task InitializeAllModulesAsync(IServiceProvider serviceProvider)
    {
        var order = GetExecutionOrder();

        foreach (var module in order)
        {
            Console.WriteLine($"[ModuleLoader] Инициализация модуля: {module.Name}");
            await module.InitializeAsync(serviceProvider);
        }
    }
}
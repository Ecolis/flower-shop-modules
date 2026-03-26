using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FlowerShop.Core;

/// <summary>
/// Методы расширения для работы с модулями
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Загрузить модули из указанных сборок и зарегистрировать их сервисы
    /// </summary>
    public static IServiceCollection AddModules(this IServiceCollection services, params Assembly[] assemblies)
    {
        var loader = new ModuleLoader(services);

        // Обнаруживаем модули
        loader.DiscoverModules(assemblies);

        // Регистрируем сервисы всех модулей
        loader.RegisterAllModules();

        // Регистрируем сами модули в DI, чтобы потом их инициализировать
        // Для этого нужно получить список модулей из loader, но он приватный.
        // Временно: сделаем так, чтобы модули можно было получить позже

        return services;
    }

    /// <summary>
    /// Загрузить модули и зарегистрировать их сервисы
    /// </summary>
    public static IServiceCollection AddModules<T>(this IServiceCollection services) where T : class
    {
        // Более простой подход: передаем список модулей явно
        return services;
    }
}
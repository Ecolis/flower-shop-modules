using Microsoft.Extensions.DependencyInjection;

namespace FlowerShop.Core;

/// <summary>
/// Контракт модуля расширения.
/// Все модули приложения должны реализовывать этот интерфейс.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Уникальное имя модуля
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Список имен модулей, от которых зависит данный модуль
    /// </summary>
    IReadOnlyList<string> Dependencies { get; }

    /// <summary>
    /// Регистрация служб модуля в DI-контейнере.
    /// Вызывается при загрузке модуля до инициализации.
    /// </summary>
    /// <param name="services">Коллекция служб для регистрации</param>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Инициализация модуля.
    /// Вызывается после разрешения всех зависимостей.
    /// Здесь модуль может выполнить начальную настройку,
    /// загрузить данные и т.д.
    /// </summary>
    /// <param name="serviceProvider">Провайдер служб для получения зависимостей</param>
    Task InitializeAsync(IServiceProvider serviceProvider);
}
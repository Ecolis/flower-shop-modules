namespace FlowerShop.Core;

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
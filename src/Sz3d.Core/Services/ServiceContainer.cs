namespace Sz3d.Core.Services;

/// <summary>轻量服务容器，用于向执行上下文注入宿主能力。</summary>
public sealed class ServiceContainer : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();

    public void Add<T>(T instance) where T : class => _services[typeof(T)] = instance;

    public object? GetService(Type serviceType)
        => _services.TryGetValue(serviceType, out var s) ? s : null;
}

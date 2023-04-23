using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nowy.Database.Contract.Services;
using Nowy.MessageHub.Client.Services;

namespace Nowy.MessageHub.Client;

public static class ServiceCollectionExtensions
{
    public static void AddNowyMessageHubClient(this IServiceCollection services, Action<INowyMessageHubConfig>? config_action)
    {
        NowyMessageHubConfig config = new();
        config_action?.Invoke(config);
        config.Apply(services);

        SocketIOConfig socket_io_config = new();
        config.Apply(socket_io_config);
        services.AddSingleton<SocketIOConfig>(sp => socket_io_config);

        services.AddSingleton<INowyMessageHub, DefaultNowyMessageHub>(sp => new DefaultNowyMessageHub(sp.GetRequiredService<SocketIOService>()));

        services.AddSingleton<SocketIOService>(sp => new SocketIOService(
            sp.GetRequiredService<ILogger<SocketIOService>>(),
            sp.GetRequiredService<SocketIOConfig>(),
            sp.GetRequiredService<IEnumerable<INowyMessageHubReceiver>>()
        ));
        services.AddHostedServiceByWrapper<SocketIOService>();

        services.AddSingleton<INowyMessageHubEventQueue>(sp => sp.GetRequiredService<DefaultNowyMessageHubEventQueue>());
        services.AddSingleton<DefaultNowyMessageHubEventQueue>(sp => new DefaultNowyMessageHubEventQueue(
            sp.GetRequiredService<ILogger<DefaultNowyMessageHubEventQueue>>(),
            sp.GetRequiredService<INowyMessageHub>()
        ));
        services.AddHostedServiceByWrapper<DefaultNowyMessageHubEventQueue>();
    }
}

public interface INowyMessageHubConfig
{
    void AddReceiver<TReceiver>(Func<IServiceProvider, TReceiver>? factory = null) where TReceiver : class, INowyMessageHubReceiver;
    void AddEndpoint(string url, Action<INowyMessageHubEndpointConfig>? endpoint_config_func = null);
}

public interface INowyMessageHubEndpointConfig
{
    void SetFilterOutgoing(Func<string, bool> func);
}

internal sealed class NowyMessageHubEndpointConfig : INowyMessageHubEndpointConfig
{
    private readonly string _url;

    private Func<string, bool>? _func;

    public NowyMessageHubEndpointConfig(string url)
    {
        this._url = url;
    }

    public string Url => this._url;

    public void SetFilterOutgoing(Func<string, bool>? func)
    {
        this._func = func;
    }

    public bool IsEventNameAllowed(string event_name)
    {
        return this._func?.Invoke(event_name) ?? true;
    }
}

internal sealed class NowyMessageHubConfig : INowyMessageHubConfig
{
    private Action<IServiceCollection>? _services_apply_func;

    public List<NowyMessageHubEndpointConfig> Endpoints { get; set; } = new();
    public TimeSpan? ConnectionRetryDelay { get; set; }

    internal void Apply(IServiceCollection services)
    {
        this._services_apply_func?.Invoke(services);
    }

    internal void Apply(SocketIOConfig socket_io_config)
    {
        socket_io_config.Endpoints = this.Endpoints;

        if (this.ConnectionRetryDelay.HasValue)
        {
            socket_io_config.ConnectionRetryDelay = this.ConnectionRetryDelay.Value;
        }
    }

    public void AddReceiver<TReceiver>(Func<IServiceProvider, TReceiver>? factory = null)
        where TReceiver : class, INowyMessageHubReceiver
    {
        this._services_apply_func += services =>
        {
            if (factory is { })
            {
                services.AddSingleton<TReceiver>(factory);
            }
            else
            {
                services.AddSingleton<TReceiver>();
            }

            services.AddSingleton<INowyMessageHubReceiver>(sp => sp.GetRequiredService<TReceiver>());
        };
    }

    public void AddEndpoint(string url, Action<INowyMessageHubEndpointConfig>? endpoint_config_func)
    {
        NowyMessageHubEndpointConfig config = new(url: url);
        endpoint_config_func?.Invoke(config);
        this.Endpoints.Add(config);
    }
}

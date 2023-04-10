using Microsoft.Extensions.DependencyInjection;
using Nowy.Auth.Client;

namespace Nowy.Auth.ClientWasm;

public static class ServiceCollectionExtensions
{
    public static void AddNowyAuthClientWasm(this IServiceCollection services, string endpoint)
    {
        services.AddNowyAuthClient(endpoint: endpoint);
    }
}

using Microsoft.Extensions.DependencyInjection;
using Nowy.Auth.Client.Services;
using Nowy.Auth.Contract.Services;

namespace Nowy.Auth.Client;

public static class ServiceCollectionExtensions
{
    public static void AddNowyAuthClient(this IServiceCollection services, string endpoint)
    {
        services.AddTransient<INowyAuthService, RestNowyAuthService>(sp => new RestNowyAuthService(
            sp.GetRequiredService<IHttpClientFactory>().CreateClient(""),
            endpoint
        ));
    }
}

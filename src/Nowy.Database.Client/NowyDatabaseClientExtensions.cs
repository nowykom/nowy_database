using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Nowy.Database.Client.Services;
using Nowy.Standard;

namespace Nowy.Database.Client;

public static class NowyDatabaseClientExtensions
{
    public static void AddNowyDatabaseClient(this IServiceCollection services, string endpoint)
    {
        services.AddTransient<INowyDatabase>(sp => new RestNowyDatabase(sp.GetRequiredService<IHttpClientFactory>(), endpoint));
    }
}

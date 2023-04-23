using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Nowy.Database.Client.Services;
using Nowy.Database.Common.Services;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;
using Nowy.Standard;

namespace Nowy.Database.Client;

public static class ServiceCollectionExtensions
{
    public static void AddNowyDatabaseClient(this IServiceCollection services, string endpoint)
    {
        services.AddSingleton<IModelService, DefaultModelService>(sp => new DefaultModelService());
        services.AddSingleton<IDatabaseEventService, DefaultDatabaseEventService>(sp => new DefaultDatabaseEventService());

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
        {
            services.AddHostedService<INowyDatabaseCacheService>(sp => new DefaultNowyDatabaseCacheService(
                sp.GetRequiredService<ILogger<DefaultNowyDatabaseCacheService>>(),
                sp.GetRequiredService<INowyDatabase>(),
                sp.GetRequiredService<IEnumerable<IDatabaseStaticDataImporter>>()
            ));
        }

        services.AddTransient<INowyDatabase>(sp => new RestNowyDatabase(
            sp.GetRequiredService<IHttpClientFactory>(),
            sp.GetService<INowyDatabaseAuthService>(),
            sp.GetService<INowyDatabaseCacheService>(),
            sp.GetRequiredService<IModelService>(),
            sp.GetRequiredService<IDatabaseEventService>(),
            endpoint
        ));


        services.AddSingleton<DatabaseEventReceiver>(sp => new DatabaseEventReceiver(
            sp.GetRequiredService<ILogger<DatabaseEventReceiver>>(),
            sp.GetRequiredService<IDatabaseEventService>()
        ));
        services.AddSingleton<INowyMessageHubReceiver>(sp => sp.GetRequiredService<DatabaseEventReceiver>());
    }
}

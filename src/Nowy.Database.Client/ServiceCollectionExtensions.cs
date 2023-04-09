﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Nowy.Database.Client.Services;
using Nowy.Database.Contract.Models;
using Nowy.Standard;

namespace Nowy.Database.Client;

public static class ServiceCollectionExtensions
{
    public static void AddNowyDatabaseClient(this IServiceCollection services, string endpoint, Func<IServiceProvider, INowyDatabaseAuthService> func_database_auth_service)
    {
        services.AddSingleton<IModelService, ModelService>(sp => new ModelService());

        services.AddTransient<INowyDatabaseAuthService>(func_database_auth_service);
        services.AddTransient<INowyDatabase>(sp => new RestNowyDatabase(
            sp.GetRequiredService<IHttpClientFactory>(),
            sp.GetRequiredService<INowyDatabaseAuthService>(),
            sp.GetRequiredService<IModelService>(),
            endpoint
        ));
    }
}
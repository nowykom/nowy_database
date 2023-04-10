using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nowy.Auth.Common.Services;
using Nowy.Auth.Contract.Services;
using Nowy.Auth.Server.Services;
using Nowy.Database.Contract.Services;

namespace Nowy.Auth.Server;

public static class ServiceCollectionExtensions
{
    public static void AddNowyAuthServer(this IServiceCollection services, string? database_name = null)
    {
        services.AddSingleton<INowyAuthService, DefaultNowyAuthService>();
        services.AddSingleton<INowyDatabaseAuthService, DefaultNowyDatabaseAuthService>();

        services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true; // <- great for debugging

            options.TokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                RequireExpirationTime = true, // <- JWTs are required to have "exp" property set
                ValidateLifetime = true, // <- the "exp" will be validated
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "nowy",
                ValidAudience = "nowy",
                IssuerSigningKey = KeyHelper.GetSymmetricKey(),
            };
        });

        services.AddAuthorization();

        services.AddSingleton<DefaultUserRepositoryConfig>(sp =>
        {
            DefaultUserRepositoryConfig config = new();

            if (!string.IsNullOrEmpty(database_name))
            {
                config.DatabaseName = database_name;
            }

            return config;
        });
        services.AddSingleton<IUserRepository, DefaultUserRepository>();
    }

    public static void UseNowyAuthServer(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseMiddleware<PermissionsMiddleware>(); // our custom permission middleware
        app.UseAuthorization();
    }
}

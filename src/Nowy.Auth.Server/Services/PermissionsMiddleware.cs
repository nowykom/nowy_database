using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nowy.Auth.Common;

namespace Nowy.Auth.Server.Services;

public class PermissionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionsMiddleware> _logger;

    public PermissionsMiddleware(
        RequestDelegate next,
        ILogger<PermissionsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserPermissionService permissionService)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            await _next(context);
            return;
        }

        CancellationToken cancellation_token = context.RequestAborted;

        string? user_sub = context.User.FindFirst(StandardJwtClaimTypes.Subject)?.Value;
        if (string.IsNullOrEmpty(user_sub))
        {
            await context.WriteAccessDeniedResponse("User 'sub' claim is required", cancellation_token: cancellation_token);
            return;
        }

        ClaimsIdentity? permissions_identity = await permissionService.GetUserPermissionsIdentity(user_sub, cancellation_token);
        if (permissions_identity == null)
        {
            _logger.LogWarning("User {sub} does not have permissions", user_sub);

            await context.WriteAccessDeniedResponse(cancellation_token: cancellation_token);
            return;
        }

        // User has permissions, so we add the extra identity containing the "permissions" claims
        context.User.AddIdentity(permissions_identity);
        await _next(context);
    }
}

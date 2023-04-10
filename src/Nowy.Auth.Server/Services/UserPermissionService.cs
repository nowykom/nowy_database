using System.Security.Claims;
using Nowy.Auth.Common;
using Nowy.Auth.Common.Models;
using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Server.Services;

public interface IUserPermissionService
{
    /// <summary>
    /// Returns a new identity containing the user permissions as Claims
    /// </summary>
    /// <param name="sub">The user external id (sub claim)</param>
    /// <param name="cancellationToken"></param>
    ValueTask<ClaimsIdentity?> GetUserPermissionsIdentity(string sub, CancellationToken cancellationToken);
}

internal class UserPermissionService : IUserPermissionService
{
    private readonly IUserRepository _user_repository;

    public UserPermissionService(IUserRepository user_repository)
    {
        _user_repository = user_repository;
    }

    public async ValueTask<ClaimsIdentity?> GetUserPermissionsIdentity(string sub, CancellationToken cancellationToken)
    {
        IReadOnlyList<IUserModel> users = await _user_repository.FindUsersAsync(sub);
        IReadOnlyList<Claim> claims = users
            .SelectMany(o => o.Permissions)
            .Select(o => new Claim(NowyClaimTypes.NowyPermissions, o))
            .ToArray();

        if (claims.Any())
        {
            ClaimsIdentity permissions_identity = new(nameof(IUserPermissionService), "name", "role");
            permissions_identity.AddClaims(claims);

            return permissions_identity;
        }

        return null;
    }
}

using System.Security.Claims;
using Nowy.Auth.Common;
using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Server.Services;

internal class DefaultUserPermissionService : IUserPermissionService
{
    private readonly IUserRepository _user_repository;

    public DefaultUserPermissionService(IUserRepository user_repository)
    {
        _user_repository = user_repository;
    }

    public async ValueTask<ClaimsIdentity?> GetUserPermissionsIdentity(string sub, CancellationToken cancellationToken)
    {
        IReadOnlyList<IUserModel> users = await this._user_repository.FindUsersAsync(sub);
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

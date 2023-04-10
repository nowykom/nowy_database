using Nowy.Auth.Contract.Services;
using Nowy.Database.Contract.Services;

namespace Nowy.Auth.Common.Services;

public class DefaultNowyDatabaseAuthService : INowyDatabaseAuthService
{
    private readonly INowyAuthService _auth_service;

    public DefaultNowyDatabaseAuthService(INowyAuthService auth_service)
    {
        _auth_service = auth_service;
    }

    public string? GetJWT()
    {
        return this._auth_service.State.JWT;
    }
}

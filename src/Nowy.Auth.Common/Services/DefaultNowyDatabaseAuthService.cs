using Nowy.Auth.Contract.Services;
using Nowy.Database.Contract.Services;

namespace Nowy.Auth.Common.Services;

public class DefaultNowyDatabaseAuthService : INowyDatabaseAuthService
{
    private readonly INowyAuthStateProvider _auth_state_provider;

    public DefaultNowyDatabaseAuthService(INowyAuthStateProvider auth_state_provider)
    {
        _auth_state_provider = auth_state_provider;
    }

    public string? GetJWT()
    {
        return _auth_state_provider.State.JWT;
    }
}

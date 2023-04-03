using Nowy.Database.Client.Services;

namespace Nowy.Database.Client.Tests.Tests;

public class MockDatabaseAuthService : INowyDatabaseAuthService
{
    public string? GetJWT()
    {
        return null;
    }
}

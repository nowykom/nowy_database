using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Contract.Services;

public interface INowyAuthService
{
    Task<INowyAuthState> LoginAsync(string name, string password);
    Task LogoutAsync();
}

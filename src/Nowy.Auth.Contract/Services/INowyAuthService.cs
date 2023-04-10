using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Contract.Services;

public interface INowyAuthService
{
    INowyAuthState State { get; }
    Task<INowyAuthState> LoginAsync(string name, string password);
    Task LogoutAsync();
}

public interface INowyAuthState
{
    bool IsAuthenticated { get; }
    string? JWT { get; }
    IReadOnlyList<string>? Errors { get; }
    IReadOnlyList<string>? UserErrors { get; }
}

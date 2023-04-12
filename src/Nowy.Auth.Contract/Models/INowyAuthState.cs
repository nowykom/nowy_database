namespace Nowy.Auth.Contract.Models;

public interface INowyAuthState
{
    bool IsAuthenticated { get; }
    string? JWT { get; }
    IReadOnlyList<string>? Errors { get; }
    IReadOnlyList<string>? UserErrors { get; }
}

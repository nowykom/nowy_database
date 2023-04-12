using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Server.Services;

internal class DefaultNowyAuthState : INowyAuthState
{
    public bool IsAuthenticated { get; init; }
    public string? JWT { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public IReadOnlyList<string>? UserErrors { get; init; }
}

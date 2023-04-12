using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Client.Services;

internal class RestNowyAuthState : INowyAuthState
{
    public bool IsAuthenticated { get; init; }
    public string? JWT { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public IReadOnlyList<string>? UserErrors { get; init; }
}

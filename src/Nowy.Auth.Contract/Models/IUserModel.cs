namespace Nowy.Auth.Contract.Models;

public interface IUserModel
{
    string Id { get; }
    string Name { get; }
    IReadOnlyList<string> Names { get; }
    IReadOnlyList<string> Permissions { get; }
    bool TryCheckPasswordAsync(string password);
}

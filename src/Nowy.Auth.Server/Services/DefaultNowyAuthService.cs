using Nowy.Auth.Contract.Models;
using Nowy.Auth.Contract.Services;

namespace Nowy.Auth.Server.Services;

public class DefaultNowyAuthService : INowyAuthService
{
    private readonly IUserRepository _user_repo;

    private DefaultServerNowyAuthState _state = new DefaultServerNowyAuthState();

    public DefaultNowyAuthService(IUserRepository user_repo)
    {
        _user_repo = user_repo;
    }

    public INowyAuthState State => _state;

    public async Task<INowyAuthState> LoginAsync(string user_name, string password)
    {
        List<IUserModel> users_success = new();

        IReadOnlyList<IUserModel> users = await _user_repo.FindUsersAsync(user_name);
        foreach (IUserModel user in users)
        {
            if (user.TryCheckPasswordAsync(password))
            {
                users_success.Add(user);
            }
        }

        List<string> errors = new();
        List<string> user_errors = new();

        if (users.Count == 0)
        {
            errors.Add($"User not found.");
            user_errors.Add($"User not found.");
        }

        if (users.Count != 0 && users_success.Count == 0)
        {
            errors.Add($"Password is not correct.");
            user_errors.Add($"Password is not correct.");
        }

        return new DefaultServerNowyAuthState
        {
            IsAuthenticated = users_success.Count != 0,
            Errors = errors,
            UserErrors = user_errors,
        };
    }

    public Task LogoutAsync()
    {
        _state = new();
        return Task.CompletedTask;
    }

    internal class DefaultServerNowyAuthState : INowyAuthState
    {
        public bool IsAuthenticated { get; init; }
        public string? JWT { get; init; }
        public IReadOnlyList<string>? Errors { get; init; }
        public IReadOnlyList<string>? UserErrors { get; init; }
    }
}

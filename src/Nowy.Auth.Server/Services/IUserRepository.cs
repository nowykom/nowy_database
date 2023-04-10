using Nowy.Auth.Common.Models;
using Nowy.Auth.Contract.Models;

namespace Nowy.Auth.Server.Services;

public interface IUserRepository
{
    ValueTask<IUserModel?> FindUserAsync(string name);
    ValueTask<IReadOnlyList<IUserModel>> FindUsersAsync(string name);
}

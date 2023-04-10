using Nowy.Auth.Common.Models;
using Nowy.Auth.Contract.Models;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Auth.Server.Services;

internal class DefaultUserRepositoryConfig
{
    public string DatabaseName { get; set; } = "nowy";
}

internal class DefaultUserRepository : IUserRepository
{
    private readonly INowyDatabase _database;
    private readonly DefaultUserRepositoryConfig _config;

    public DefaultUserRepository(INowyDatabase database, DefaultUserRepositoryConfig config)
    {
        _database = database;
        _config = config;
    }


    public async ValueTask<IUserModel?> FindUserAsync(string name)
    {
        IReadOnlyList<DefaultUserModel> users = await _database.GetCollection<DefaultUserModel>(_config.DatabaseName).GetAllAsync();
        return users.FirstOrDefault(o => o.Names.Contains(name));
    }

    public async ValueTask<IReadOnlyList<IUserModel>> FindUsersAsync(string name)
    {
        IReadOnlyList<DefaultUserModel> users = await _database.GetCollection<DefaultUserModel>(_config.DatabaseName).GetAllAsync();
        return users.Where(o => o.Names.Contains(name)).ToList();
    }
}

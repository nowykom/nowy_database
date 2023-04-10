using Nowy.Database.Common.Services;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Web.Services;

internal class MongoNowyDatabase : INowyDatabase
{
    private readonly ILogger _logger;
    private readonly MongoRepository _repo;

    public MongoNowyDatabase(ILogger<MongoNowyDatabase> logger, MongoRepository repo)
    {
        _logger = logger;
        _repo = repo;
    }

    public INowyCollection<TModel> GetCollection<TModel>(string database_name) where TModel : class, IBaseModel
    {
        string entity_name = EntityNameHelper.GetEntityName(typeof(TModel));

        return new MongoNowyCollection<TModel>(
            logger: _logger,
            repo: _repo,
            database_name: database_name,
            entity_name: entity_name
        );
    }
}

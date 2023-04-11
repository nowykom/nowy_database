using Nowy.Database.Common.Services;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Client.Services;

internal sealed class RestNowyDatabase : INowyDatabase
{
    private readonly HttpClient _http_client;
    private readonly INowyDatabaseAuthService? _database_auth_service;
    private readonly INowyDatabaseCacheService? _database_cache_service;
    private readonly IModelService _model_service;
    private readonly string _endpoint;

    public RestNowyDatabase(
        IHttpClientFactory http_client_factory,
        INowyDatabaseAuthService? database_auth_service,
        INowyDatabaseCacheService? database_cache_service,
        IModelService model_service,
        string endpoint
    )
    {
        _http_client = http_client_factory.CreateClient("");
        _database_auth_service = database_auth_service;
        _database_cache_service = database_cache_service;
        _model_service = model_service;
        _endpoint = endpoint;
    }

    public INowyCollection<TModel> GetCollection<TModel>(string database_name) where TModel : class, IBaseModel
    {
        return GetCollection<TModel>(database_name: database_name, cached: false);
    }

    public INowyCollection<TModel> GetCollection<TModel>(string database_name, bool cached) where TModel : class, IBaseModel
    {
        string entity_name = EntityNameHelper.GetEntityName(typeof(TModel));

        if (cached && _database_cache_service is not null)
        {
            return new CachedNowyCollection<TModel>(
                _http_client,
                _database_cache_service,
                _model_service,
                _endpoint,
                database_name: database_name,
                entity_name: entity_name
            );
        }

        return new RestNowyCollection<TModel>(
            _http_client,
            _database_auth_service,
            _model_service,
            _endpoint,
            database_name: database_name,
            entity_name: entity_name
        );
    }
}

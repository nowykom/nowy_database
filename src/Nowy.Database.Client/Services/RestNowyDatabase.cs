using Nowy.Database.Common.Services;
using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

internal sealed class RestNowyDatabase : INowyDatabase
{
    private readonly HttpClient _http_client;
    private readonly INowyDatabaseAuthService _database_auth_service;
    private readonly IModelService _model_service;
    private readonly string _endpoint;

    public RestNowyDatabase(
        IHttpClientFactory http_client_factory,
        INowyDatabaseAuthService database_auth_service,
        IModelService model_service,
        string endpoint
    )
    {
        _http_client = http_client_factory.CreateClient("");
        _database_auth_service = database_auth_service;
        _model_service = model_service;
        _endpoint = endpoint;
    }

    public INowyCollection<TModel> GetCollection<TModel>(string database_name) where TModel : class, IBaseModel
    {
        string entity_name = EntityNameHelper.GetEntityName(typeof(TModel));

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

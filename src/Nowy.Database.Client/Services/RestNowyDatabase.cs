using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

public sealed class RestNowyDatabase : INowyDatabase
{
    private readonly HttpClient _http_client;
    private readonly string _endpoint;

    public RestNowyDatabase(IHttpClientFactory http_client_factory, string endpoint)
    {
        _http_client = http_client_factory.CreateClient("");
        _endpoint = endpoint;
    }

    public INowyCollection<TModel> GetCollection<TModel>(string database_name) where TModel : class, IBaseModel
    {
        return new RestNowyCollection<TModel>(_http_client, _endpoint, database_name: database_name, entity_name: typeof(TModel).Name.Replace("Model", string.Empty));
    }
}

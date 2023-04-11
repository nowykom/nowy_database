using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Client.Services;

internal sealed class CachedNowyCollection<TModel> : INowyCollection<TModel> where TModel : class, IBaseModel
{
    private readonly HttpClient _http_client;
    private readonly INowyDatabaseCacheService nowyDatabaseCacheService;
    private readonly IModelService _model_service;
    private readonly string _endpoint;
    private readonly string _database_name;
    private readonly string _entity_name;

    private static readonly JsonSerializerOptions _json_options = new JsonSerializerOptions() { PropertyNamingPolicy = null, };

    public CachedNowyCollection(
        HttpClient http_client,
        INowyDatabaseCacheService nowyDatabaseCacheService,
        IModelService model_service,
        string endpoint,
        string database_name,
        string entity_name
    )
    {
        _http_client = http_client;
        this.nowyDatabaseCacheService = nowyDatabaseCacheService;
        _model_service = model_service;
        _endpoint = endpoint;
        _database_name = database_name;
        _entity_name = entity_name;
    }

    public async Task<IReadOnlyList<TModel>> GetAllAsync()
    {
        return this.nowyDatabaseCacheService.Fetch<TModel>();
    }

    public Task<TModel?> GetByIdAsync(string id)
    {
        TModel? o = this.nowyDatabaseCacheService.FetchById<TModel>(id);
        return Task.FromResult(o);
    }

    public Task<TModel> InsertAsync(string id, TModel model)
    {
        this.nowyDatabaseCacheService.Add(model);
        this.nowyDatabaseCacheService.Save();
        return Task.FromResult(model);
    }

    public Task<TModel> UpdateAsync(string id, TModel model)
    {
        this.nowyDatabaseCacheService.Add(model);
        this.nowyDatabaseCacheService.Save();
        return Task.FromResult(model);
    }

    public async Task DeleteAsync(string id)
    {
        TModel? o = this.nowyDatabaseCacheService.FetchById<TModel>(id);
        if (o is { })
        {
            this.nowyDatabaseCacheService.Delete(o);
        }

        this.nowyDatabaseCacheService.Save();
    }
}

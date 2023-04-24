using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MongoDB.Bson;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Web.Services;

internal sealed class MongoNowyCollection<TModel> : INowyCollection<TModel> where TModel : class, IBaseModel
{
    private readonly ILogger _logger;
    private readonly MongoRepository _repo;
    private readonly string _database_name;
    private readonly string _entity_name;

    private static readonly JsonSerializerOptions _json_options = new JsonSerializerOptions() { PropertyNamingPolicy = null, };

    public MongoNowyCollection(
        ILogger logger,
        MongoRepository repo,
        string database_name,
        string entity_name
    )
    {
        _logger = logger;
        _repo = repo;
        _database_name = database_name;
        _entity_name = entity_name;
    }

    string INowyCollection<TModel>.DatabaseName => this._database_name;
    string INowyCollection<TModel>.EntityName => this._entity_name;

    public async Task<IReadOnlyList<TModel>> GetAllAsync()
    {
        List<BsonDocument> list = await _repo.GetAllAsync(database_name: _database_name, entity_name: _entity_name);

        await using MemoryStream stream = await list.MongoToJsonStream();

        List<TModel>? result = await JsonSerializer.DeserializeAsync<List<TModel>>(stream, options: _json_options) ?? new();
        return result;
    }

    public async Task<IReadOnlyList<TModel>> GetByFilterAsync(ModelFilter filter)
    {
        List<BsonDocument> list = await _repo.GetByFilterAsync(database_name: _database_name, entity_name: _entity_name, filter: filter);

        await using MemoryStream stream = await list.MongoToJsonStream();

        List<TModel>? result = await JsonSerializer.DeserializeAsync<List<TModel>>(stream, options: _json_options) ?? new();
        return result;
    }

    public async Task<TModel?> GetByIdAsync(string id)
    {
        BsonDocument? document = await _repo.GetByIdAsync(database_name: _database_name, entity_name: _entity_name, id: id);

        if (document is { })
        {
            await using MemoryStream stream = await document.MongoToJsonStream();

            TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
            return result;
        }
        else
        {
            return null;
        }
    }

    public async Task<TModel> InsertAsync(string id, TModel model)
    {
        string input_json = JsonSerializer.Serialize(model, _json_options);

        BsonDocument input_document = BsonDocument.Parse(input_json);
        BsonDocument? document = await _repo.UpsertAsync(database_name: _database_name, entity_name: _entity_name, id: id, input: input_document);
        await using MemoryStream stream = await document.MongoToJsonStream();

        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        return result ?? throw new ArgumentNullException(nameof(result));
    }

    public async Task<TModel> UpdateAsync(string id, TModel model)
    {
        string input_json = JsonSerializer.Serialize(model, _json_options);

        BsonDocument input_document = BsonDocument.Parse(input_json);
        BsonDocument? document = await _repo.UpsertAsync(database_name: _database_name, entity_name: _entity_name, id: id, input: input_document);
        await using MemoryStream stream = await document.MongoToJsonStream();

        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        return result ?? throw new ArgumentNullException(nameof(result));
    }

    public async Task DeleteAsync(string id)
    {
        BsonDocument? document = await _repo.DeleteAsync(database_name: _database_name, entity_name: _entity_name, id: id);
    }

    public INowyCollectionEventSubscription<TModel> Subscribe()
    {
        return new NullNowyCollectionEventSubscription<TModel>();
    }
}

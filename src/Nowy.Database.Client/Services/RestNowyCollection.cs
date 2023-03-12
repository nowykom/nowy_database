using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

public sealed class RestNowyCollection<TModel> : INowyCollection<TModel> where TModel : class, IBaseModel
{
    private readonly HttpClient _http_client;
    private readonly string _endpoint;
    private readonly string _database_name;
    private readonly string _entity_name;

    private static readonly JsonSerializerOptions _json_options = new JsonSerializerOptions() { PropertyNamingPolicy = null, };

    public RestNowyCollection(HttpClient http_client, string endpoint, string database_name, string entity_name)
    {
        _http_client = http_client;
        _endpoint = endpoint;
        _database_name = database_name;
        _entity_name = entity_name;
    }

    public async Task<IReadOnlyList<TModel>> GetAllAsync()
    {
        await using Stream stream = await _http_client.GetStreamAsync($"{_endpoint}/api/v1/{_database_name}/{_entity_name}");
        List<TModel>? result = await JsonSerializer.DeserializeAsync<List<TModel>>(stream, options: _json_options) ?? new();
        return result;
    }

    public async Task<TModel?> GetByIdAsync(string uuid)
    {
        string a = $"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}";
        using HttpResponseMessage response = await _http_client.GetAsync($"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}");
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync();
        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        return result;
    }

    public async Task<TModel> InsertAsync(string uuid, TModel model)
    {
        using HttpResponseMessage response = await _http_client.PostAsJsonAsync($"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}", model, options: _json_options);
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        return result ?? throw new ArgumentNullException(nameof(result));
    }

    public async Task<TModel> UpdateAsync(string uuid, TModel model)
    {
        using HttpResponseMessage response = await _http_client.PostAsJsonAsync($"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}", model, options: _json_options);
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        return result ?? throw new ArgumentNullException(nameof(result));
    }

    public async Task DeleteAsync(string uuid)
    {
        using HttpResponseMessage response = await _http_client.DeleteAsync($"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}");
        string result = await response.Content.ReadAsStringAsync();
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

public sealed class RestNowyCollection<TModel> : INowyCollection<TModel> where TModel : class, IBaseModel
{
    private readonly HttpClient _http_client;
    private readonly INowyDatabaseAuthService _database_auth_service;
    private readonly IModelService _model_service;
    private readonly string _endpoint;
    private readonly string _database_name;
    private readonly string _entity_name;

    private static readonly JsonSerializerOptions _json_options = new JsonSerializerOptions() { PropertyNamingPolicy = null, };

    public RestNowyCollection(
        HttpClient http_client,
        INowyDatabaseAuthService database_auth_service,
        IModelService model_service,
        string endpoint,
        string database_name,
        string entity_name
    )
    {
        _http_client = http_client;
        _database_auth_service = database_auth_service;
        _model_service = model_service;
        _endpoint = endpoint;
        _database_name = database_name;
        _entity_name = entity_name;
    }

    private void _configureAuth(HttpRequestMessage request)
    {
        if (_database_auth_service.GetJWT() is { Length: > 0 } jwt)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }
        else
        {
            request.Headers.Authorization = null;
        }
    }

    public async Task<IReadOnlyList<TModel>> GetAllAsync()
    {
        string url = $"{_endpoint}/api/v1/{_database_name}/{_entity_name}";
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        _configureAuth(request);

        using HttpResponseMessage response = await _http_client.SendAsync(request);
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        List<TModel>? result = await JsonSerializer.DeserializeAsync<List<TModel>>(stream, options: _json_options) ?? new();
        return result;
    }

    public async Task<TModel?> GetByIdAsync(string uuid)
    {
        string url = $"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}";
        using HttpRequestMessage request = new(HttpMethod.Get, url);
        _configureAuth(request);

        using HttpResponseMessage response = await _http_client.SendAsync(request);
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
        string url = $"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}";
        using HttpRequestMessage request = new(HttpMethod.Post, url);
        _configureAuth(request);
        request.Content = JsonContent.Create(model, mediaType: null, _json_options);

        using HttpResponseMessage response = await _http_client.SendAsync(request);
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        _model_service.SendModelUpdated(model);
        return result ?? throw new ArgumentNullException(nameof(result));
    }

    public async Task<TModel> UpdateAsync(string uuid, TModel model)
    {
        string url = $"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}";
        using HttpRequestMessage request = new(HttpMethod.Post, url);
        _configureAuth(request);
        request.Content = JsonContent.Create(model, mediaType: null, _json_options);

        using HttpResponseMessage response = await _http_client.SendAsync(request);
        await using Stream stream = await response.Content.ReadAsStreamAsync();
        TModel? result = await JsonSerializer.DeserializeAsync<TModel>(stream, options: _json_options);
        return result ?? throw new ArgumentNullException(nameof(result));
    }

    public async Task DeleteAsync(string uuid)
    {
        string url = $"{_endpoint}/api/v1/{_database_name}/{_entity_name}/{uuid}";
        using HttpRequestMessage request = new(HttpMethod.Delete, url);
        _configureAuth(request);

        using HttpResponseMessage response = await _http_client.SendAsync(request);
        string result = await response.Content.ReadAsStringAsync();
    }
}

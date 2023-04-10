using System.Text.Json;
using Nowy.Auth.Common.Models;
using Nowy.Auth.Contract.Services;

namespace Nowy.Auth.Client.Services;

public class RestNowyAuthService : INowyAuthService
{
    private readonly HttpClient _http_client;
    private readonly string _endpoint;

    private static readonly JsonSerializerOptions _json_options = new JsonSerializerOptions() { PropertyNamingPolicy = null, };

    private RestNowyAuthState _state = new RestNowyAuthState();

    public RestNowyAuthService(
        HttpClient http_client,
        string endpoint
    )
    {
        _http_client = http_client;
        _endpoint = endpoint;
    }

    public INowyAuthState State => _state;

    public async Task<INowyAuthState> LoginAsync(string name, string password)
    {
        try
        {
            string url = $"{_endpoint}/api/v1/auth/login";
            using HttpRequestMessage request = new(HttpMethod.Post, url);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", name),
                new KeyValuePair<string, string>("password", password),
            });

            using HttpResponseMessage response = await _http_client.SendAsync(request);
            await using Stream stream = await response.Content.ReadAsStreamAsync();

            RestNowyAuthLoginResponse? result = await JsonSerializer.DeserializeAsync<RestNowyAuthLoginResponse>(stream, options: _json_options);

            if (result is null)
            {
                result = new()
                {
                    Errors = new()
                    {
                        $"Request failed: {nameof(RestNowyAuthLoginResponse)} is null.",
                    },
                };
            }

            _state = new()
            {
                IsAuthenticated = result.IsAuthenticated,
                JWT = result.JWT,
                Errors = result.Errors,
                UserErrors = result.UserErrors,
            };
        }
        catch (Exception ex)
        {
            _state = new()
            {
                Errors = new List<string>()
                {
                    ex.ToString(),
                },
                UserErrors = new List<string>()
                {
                    $"Error: {ex.GetType().Name}: {ex.Message}",
                }
            };
        }

        return _state;
    }

    public Task LogoutAsync()
    {
        _state = new();
        return Task.CompletedTask;
    }

    internal class RestNowyAuthState : INowyAuthState
    {
        public bool IsAuthenticated { get; init; }
        public string? JWT { get; init; }
        public IReadOnlyList<string>? Errors { get; init; }
        public IReadOnlyList<string>? UserErrors { get; init; }
    }
}

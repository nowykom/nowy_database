using System.Text.Json.Serialization;

namespace Nowy.Auth.Common.Models;

public class RestNowyAuthLoginResponse
{
    [JsonPropertyName("is_authenticated")] public bool IsAuthenticated { get; set; }
    [JsonPropertyName("errors")] public List<string>? Errors { get; set; }
    [JsonPropertyName("user_errors")] public List<string>? UserErrors { get; set; }
    [JsonPropertyName("jwt")] public string? JWT { get; set; }
}

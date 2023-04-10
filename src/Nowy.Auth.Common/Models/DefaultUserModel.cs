using System.Text.Json.Serialization;
using Nowy.Auth.Contract.Models;
using Nowy.Database.Common.Models;
using Nowy.Database.Contract.Models;

namespace Nowy.Auth.Common.Models;

[EntityName("User")]
public class DefaultUserModel : BaseModel, IUserModel
{
    private List<string>? _names;
    private List<string>? _permissions;

    [JsonIgnore]
    public string Name
    {
        get => this._names?.FirstOrDefault() ?? string.Empty;
    }

    [JsonPropertyName("names")]
    public List<string> Names
    {
        get => this._names ??= new();
        set => ( this._names ??= new() ).ReplaceWith(value ?? new());
    }


    [JsonPropertyName("permissions")]
    public List<string> Permissions
    {
        get => this._permissions ??= new();
        set => ( this._permissions ??= new() ).ReplaceWith(value ?? new());
    }

    [JsonPropertyName("email_addresses")] public List<string> EmailAddresses { get; set; } = new();

    [JsonPropertyName("meta")] public Dictionary<string, string?> Meta { get; set; } = new();


    string IUserModel.Id => this.uuid;
    string IUserModel.Name => this._names?.FirstOrDefault() ?? string.Empty;
    IReadOnlyList<string> IUserModel.Names => ( this._names as IReadOnlyList<string> ) ?? Array.Empty<string>();
    IReadOnlyList<string> IUserModel.Permissions => ( this._permissions as IReadOnlyList<string> ) ?? Array.Empty<string>();


    public bool TryCheckPasswordAsync(string password)
    {
        return true;
    }
}

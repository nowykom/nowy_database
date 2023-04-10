using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Nowy.Database.Common.Services;
using Nowy.Database.Contract.Models;

namespace Nowy.Database.Common.Models;

public abstract class BaseModel : IBaseModel
{
    private readonly ModelReflectionHelper _reflection_helpers;

    protected BaseModel()
    {
        this._reflection_helpers = ModelReflectionHelper.GetHelperInstance(instance_type: this.GetType());
    }

    private static Type? _model_type = null;

    [JsonIgnore] public Type ModelType => _model_type ??= this.GetType();

    [JsonIgnore] public bool ShouldSave { get; set; }

    [JsonPropertyName("id")] public string id { get; set; } = string.Empty;

    [JsonPropertyName("ids")] public IReadOnlyList<string> ids { get; set; } = Array.Empty<string>();

    [JsonPropertyName("timestamp_database_insert")]
    public long timestamp_database_insert { get; set; }

    [JsonPropertyName("timestamp_database_update")]
    public long timestamp_database_update { get; set; }


    public void SetField(string name, object? value, ILogger? logger = null)
    {
        try
        {
            this._reflection_helpers.SetProperty(instance: this, name: name, value: value, logger: logger);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Failed to set field '{name}' to '{value}' ({value?.GetType().Name})");
        }
    }

    public string? GetField(string name, ILogger? logger = null)
    {
        try
        {
            return this._reflection_helpers.GetProperty(instance: this, name: name, logger: logger);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Failed to get field '{name}'");
        }

        return null;
    }

    public Dictionary<string, string?> GetFields(ILogger? logger = null)
    {
        Dictionary<string, string?> ret = new();
        foreach (string name in this._reflection_helpers.GetPropertyNames())
        {
            try
            {
                string? value = this._reflection_helpers.GetProperty(instance: this, name: name, logger: logger);
                ret[name] = value;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, $"Failed to get field '{name}'");
            }
        }

        return ret;
    }

    protected string? _getMorph(string? morph_id, string? morph_type, string? php_model_class)
    {
        return morph_type == php_model_class ? morph_id : null;
    }

    protected void _setMorph(Action<string, string> morph_set_func, string php_model_class, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            morph_set_func(php_model_class, value);
        }
    }

    public virtual void ClearCaches()
    {
    }
}

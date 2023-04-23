using System.Text.Json.Serialization;

namespace Nowy.Database.Contract.Models;

public abstract class CollectionEvent
{
    [JsonPropertyName("database_name")] public string? DatabaseName { get; set; }
    [JsonPropertyName("entity_name")] public string? EntityName { get; set; }
}

public abstract class CollectionModelEvent : CollectionEvent
{
    [JsonPropertyName("id")] public string? Id { get; set; }
}

public sealed class CollectionModelInsertedEvent : CollectionModelEvent
{
}

public sealed class CollectionModelUpdatedEvent : CollectionModelEvent
{
}

public sealed class CollectionModelDeletedEvent : CollectionModelEvent
{
}

public sealed class CollectionChangedEvent : CollectionEvent
{
}

public sealed class CollectionModelsInsertedEvent : CollectionEvent
{
}

public sealed class CollectionModelsUpdatedEvent : CollectionEvent
{
}

public sealed class CollectionModelsDeletedEvent : CollectionEvent
{
}

using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

public readonly record struct EntityChangedEventArgs(string DatabaseName, string EntityName, string Id);

public readonly record struct CollectionChangedEventArgs(string DatabaseName, string EntityName);

public interface IDatabaseEventService
{
    event Action<EntityChangedEventArgs>? CollectionModelInserted;
    event Action<EntityChangedEventArgs>? CollectionModelUpdated;
    event Action<EntityChangedEventArgs>? CollectionModelDeleted;

    event Action<CollectionChangedEventArgs>? CollectionChanged;

    event Action<CollectionChangedEventArgs>? CollectionModelsInserted;
    event Action<CollectionChangedEventArgs>? CollectionModelsUpdated;
    event Action<CollectionChangedEventArgs>? CollectionModelsDeleted;

    void SendCollectionModelInserted(EntityChangedEventArgs e);
    void SendCollectionModelUpdated(EntityChangedEventArgs e);
    void SendCollectionModelDeleted(EntityChangedEventArgs e);

    void SendCollectionChanged(CollectionChangedEventArgs e);

    void SendCollectionModelsInserted(CollectionChangedEventArgs e);
    void SendCollectionModelsUpdated(CollectionChangedEventArgs e);
    void SendCollectionModelsDeleted(CollectionChangedEventArgs e);
}

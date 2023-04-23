using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

internal sealed class DefaultDatabaseEventService : IDatabaseEventService
{
    public event Action<EntityChangedEventArgs>? CollectionModelInserted;
    public event Action<EntityChangedEventArgs>? CollectionModelUpdated;
    public event Action<EntityChangedEventArgs>? CollectionModelDeleted;
    public event Action<CollectionChangedEventArgs>? CollectionChanged;
    public event Action<CollectionChangedEventArgs>? CollectionModelsInserted;
    public event Action<CollectionChangedEventArgs>? CollectionModelsUpdated;
    public event Action<CollectionChangedEventArgs>? CollectionModelsDeleted;

    public void SendCollectionModelInserted(EntityChangedEventArgs e)
    {
        CollectionModelInserted?.Invoke(e);
    }

    public void SendCollectionModelUpdated(EntityChangedEventArgs e)
    {
        CollectionModelUpdated?.Invoke(e);
    }

    public void SendCollectionModelDeleted(EntityChangedEventArgs e)
    {
        CollectionModelInserted?.Invoke(e);
    }

    public void SendCollectionChanged(CollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(e);
    }

    public void SendCollectionModelsInserted(CollectionChangedEventArgs e)
    {
        CollectionModelsInserted?.Invoke(e);
    }

    public void SendCollectionModelsUpdated(CollectionChangedEventArgs e)
    {
        CollectionModelsUpdated?.Invoke(e);
    }

    public void SendCollectionModelsDeleted(CollectionChangedEventArgs e)
    {
        CollectionModelsDeleted?.Invoke(e);
    }
}

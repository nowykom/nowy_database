using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

internal sealed class DefaultDatabaseEventService : IDatabaseEventService
{
    public event Action<CollectionChangedEventArgs>? CollectionChanged;

    public void SendCollectionChanged(CollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(e);
    }
}

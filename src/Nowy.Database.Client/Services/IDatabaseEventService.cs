using Nowy.Database.Contract.Models;

namespace Nowy.Database.Client.Services;

public readonly record struct CollectionChangedEventArgs(string DatabaseName, string EntityName);

public interface IDatabaseEventService
{
    event Action<CollectionChangedEventArgs>? CollectionChanged;
    void SendCollectionChanged(CollectionChangedEventArgs e);
}

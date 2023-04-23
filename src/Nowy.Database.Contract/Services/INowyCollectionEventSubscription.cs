using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public interface INowyCollectionEventSubscription<TModel> : INowyCollectionEventSubscription where TModel : class, IBaseModel
{
    INowyCollectionEventSubscription<TModel> AddHandler(Action handler);
    INowyCollectionEventSubscription<TModel> AddHandler(Func<ValueTask> handler);
}

public interface INowyCollectionEventSubscription : IDisposable
{
    INowyCollectionEventSubscription AddHandler(Action handler);
    INowyCollectionEventSubscription AddHandler(Func<ValueTask> handler);

    void SendCollectionModelInserted(Action<EntityChangedEventArgs> handler);
    void SendCollectionModelInserted(Func<EntityChangedEventArgs,ValueTask> handler);
    void SendCollectionModelUpdated(Action<EntityChangedEventArgs> handler);
    void SendCollectionModelUpdated(Func<EntityChangedEventArgs,ValueTask> handler);
    void SendCollectionModelDeleted(Action<EntityChangedEventArgs> handler);
    void SendCollectionModelDeleted(Func<EntityChangedEventArgs,ValueTask> handler);

    void SendCollectionChanged(Action<CollectionChangedEventArgs> handler);
    void SendCollectionChanged(Func<CollectionChangedEventArgs,ValueTask> handler);

    void SendCollectionModelsInserted(Action<CollectionChangedEventArgs> handler);
    void SendCollectionModelsInserted(Func<CollectionChangedEventArgs,ValueTask> handler);
    void SendCollectionModelsUpdated(Action<CollectionChangedEventArgs> handler);
    void SendCollectionModelsUpdated(Func<CollectionChangedEventArgs,ValueTask> handler);
    void SendCollectionModelsDeleted(Action<CollectionChangedEventArgs> handler);
    void SendCollectionModelsDeleted(Func<CollectionChangedEventArgs,ValueTask> handler);
}

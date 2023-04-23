using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public readonly record struct EntityChangedEventArgs(string DatabaseName, string EntityName, string Id);

public readonly record struct CollectionChangedEventArgs(string DatabaseName, string EntityName);

public interface INowyCollectionEventSubscription<TModel> : INowyCollectionEventSubscription where TModel : class, IBaseModel
{
    INowyCollectionEventSubscription<TModel> AddHandler<TEvent>(Action handler) where TEvent : CollectionEvent;
    INowyCollectionEventSubscription<TModel> AddHandler<TEvent>(Func<ValueTask> handler) where TEvent : CollectionEvent;
}

public interface INowyCollectionEventSubscription : IDisposable
{
    INowyCollectionEventSubscription AddHandler<TEvent>(Action handler) where TEvent : CollectionEvent;
    INowyCollectionEventSubscription AddHandler<TEvent>(Func<ValueTask> handler) where TEvent : CollectionEvent;
}

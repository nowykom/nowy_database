using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public interface INowyCollectionEventSubscription<TModel> : INowyCollectionEventSubscription where TModel : class, IBaseModel
{
    INowyCollectionEventSubscription<TModel> AddHandler(Action handler);
    INowyCollectionEventSubscription<TModel> AddHandler(Func<Task> handler);
}

public interface INowyCollectionEventSubscription : IDisposable
{
    INowyCollectionEventSubscription AddHandler(Action handler);
    INowyCollectionEventSubscription AddHandler(Func<Task> handler);
}

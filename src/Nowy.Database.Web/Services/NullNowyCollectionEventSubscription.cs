using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Web.Services;

internal class NullNowyCollectionEventSubscription<TModel> : INowyCollectionEventSubscription<TModel> where TModel : class, IBaseModel
{
    public void Dispose()
    {
    }

    public INowyCollectionEventSubscription<TModel> AddHandler<TEvent>(Action handler) where TEvent : CollectionEvent
    {
        return this;
    }

    INowyCollectionEventSubscription INowyCollectionEventSubscription.AddHandler<TEvent>(Action handler)
    {
        return this.AddHandler<TEvent>(handler);
    }

    public INowyCollectionEventSubscription<TModel> AddHandler<TEvent>(Func<ValueTask> handler) where TEvent : CollectionEvent
    {
        return this;
    }

    INowyCollectionEventSubscription INowyCollectionEventSubscription.AddHandler<TEvent>(Func<ValueTask> handler)
    {
        return this.AddHandler<TEvent>(handler);
    }
}

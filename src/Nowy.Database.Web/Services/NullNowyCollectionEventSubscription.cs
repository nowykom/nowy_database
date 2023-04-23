using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Web.Services;

internal class NullNowyCollectionEventSubscription<TModel> : INowyCollectionEventSubscription<TModel> where TModel : class, IBaseModel
{
    public void Dispose()
    {
    }

    public INowyCollectionEventSubscription<TModel> AddHandler(Action handler)
    {
        return this;
    }

    INowyCollectionEventSubscription INowyCollectionEventSubscription.AddHandler(Action handler)
    {
        return this.AddHandler(handler);
    }

    public INowyCollectionEventSubscription<TModel> AddHandler(Func<Task> handler)
    {
        return this;
    }

    INowyCollectionEventSubscription INowyCollectionEventSubscription.AddHandler(Func<Task> handler)
    {
        return this.AddHandler(handler);
    }
}

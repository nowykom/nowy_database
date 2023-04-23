using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Client.Services;

public class DefaultNowyCollectionEventSubscription<TModel> : INowyCollectionEventSubscription<TModel> where TModel : class, IBaseModel
{
    private INowyCollection<TModel>? _collection;
    private IDatabaseEventService? _event_service;
    private List<Func<Task>>? _handlers;

    public DefaultNowyCollectionEventSubscription(INowyCollection<TModel> collection, IDatabaseEventService event_service)
    {
        this._collection = collection;
        this._event_service = event_service;

        event_service.SendCollectionModelDeleted();
    }

    public INowyCollectionEventSubscription<TModel> AddHandler(Action handler)
    {
        this._handlers ??= new();
        this._handlers.Add(() =>
        {
            handler();
            return Task.CompletedTask;
        });
        return this;
    }

    INowyCollectionEventSubscription INowyCollectionEventSubscription.AddHandler(Action handler)
    {
        return this.AddHandler(handler);
    }

    public INowyCollectionEventSubscription<TModel> AddHandler(Func<Task> handler)
    {
        this._handlers ??= new();
        this._handlers.Add(handler);
        return this;
    }

    INowyCollectionEventSubscription INowyCollectionEventSubscription.AddHandler(Func<Task> handler)
    {
        return this.AddHandler(handler);
    }

    public void Dispose()
    {
        this._collection = null;
        this._event_service = null;
        this._handlers?.Clear();
        this._handlers = null;
    }
}

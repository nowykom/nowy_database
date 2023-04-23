using Nowy.Database.Contract.Models;

namespace Nowy.MessageHub.Client.Services;

internal class DefaultNowyMessageHubEventSubscriptionBuilder<TEvent> : INowyMessageHubEventSubscriptionBuilder<TEvent> where TEvent : class
{
    public INowyMessageHubEventSubscriptionBuilder<TEvent> Where(Predicate<TEvent> value)
    {

    }

    public INowyMessageHubEventSubscriptionBuilder<TEvent> AddHandler(Action<TEvent> value)
    {
    }

    public INowyMessageHubEventSubscriptionBuilder<TEvent> AddHandler(Func<TEvent, ValueTask> value)
    {
    }
}

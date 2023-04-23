namespace Nowy.Database.Contract.Models;

public interface INowyMessageHubEventSubscriptionBuilder<out TEvent> where TEvent : class
{
    INowyMessageHubEventSubscriptionBuilder<TEvent> Where(Predicate<TEvent> value);
    INowyMessageHubEventSubscriptionBuilder<TEvent> AddHandler(Action<TEvent> value);
    INowyMessageHubEventSubscriptionBuilder<TEvent> AddHandler(Func<TEvent, ValueTask> value);
}

public interface INowyMessageHubEventSubscription : IDisposable
{
}

public static class NowyMessageHubEventSubscriptionBuilderExtensions
{
}

using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;

namespace Nowy.MessageHub.Client.Services;

internal class DefaultNowyMessageHub : INowyMessageHub
{
    private readonly DefaultNowyMessageHubInternal _message_hub_internal;
    private readonly DefaultNowyMessageHubEventQueue _message_hub_event_queue;

    public DefaultNowyMessageHub(
        DefaultNowyMessageHubInternal message_hub_internal,
        DefaultNowyMessageHubEventQueue message_hub_event_queue
    )
    {
        this._message_hub_internal = message_hub_internal;
        this._message_hub_event_queue = message_hub_event_queue;
    }

    public async Task BroadcastMessageAsync(string event_name, object[] values, NowyMessageOptions? options)
    {
        await this._message_hub_internal.BroadcastMessageAsync(event_name, values, options);
    }

    public async Task WaitUntilConnectedAsync(string event_name, CancellationToken token)
    {
        await this._message_hub_internal.WaitUntilConnectedAsync(event_name, token);
    }

    public async Task WaitUntilConnectedAsync(string event_name, TimeSpan delay)
    {
        await this._message_hub_internal.WaitUntilConnectedAsync(event_name, delay);
    }

    public void QueueBroadcastMessage(string event_name, object event_value, NowyMessageOptions message_options)
    {
        _message_hub_event_queue.QueueBroadcastMessage(event_name, event_value, message_options);
    }

    public void QueueEvent(Action<INowyMessageHubEventEnvelopeBuilder> configure)
    {
        DefaultNowyMessageHubEventEnvelopeBuilder builder = new();
        configure(builder);

        string[] recipients = builder.Recipients.ToArray()

        foreach (object value in builder.Values)
        {
            this._message_hub_event_queue.QueueBroadcastMessage(
                value.GetType().Name,
                value,
                new NowyMessageOptions
                {
                    ExceptSender = false,
                    Recipients = recipients,
                }
            );
        }
    }

    public INowyMessageHubEventSubscription SubscribeEvent<TEvent>(Action<INowyMessageHubEventSubscriptionBuilder<TEvent>> configure) where TEvent : class
    {
        INowyMessageHubEventSubscriptionBuilder<TEvent> builder = new DefaultNowyMessageHubEventSubscriptionBuilder<TEvent>();
        configure(builder);


    }
}

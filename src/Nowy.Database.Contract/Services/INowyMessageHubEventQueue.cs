namespace Nowy.Database.Contract.Services;

public interface INowyMessageHubEventQueue
{
    public void QueueBroadcastMessage(string event_name, object event_value, NowyMessageOptions message_options);
}

namespace Nowy.Database.Contract.Services;

public interface INowyMessageHubEventBuilder
{
    void AddRecipient(string name);
}

public interface IMessageHubEvent
{
    IMessageHubPeer Sender { get; }
}

public static class NowyMessageHubEventBuilderExtensions
{
    public static void AddRecipient(this INowyMessageHubEventBuilder that, IEnumerable<string> names)
    {
        foreach (string name in names)
        {
            that.AddRecipient(name: name);
        }
    }

    public static void AddRecipients(this INowyMessageHubEventBuilder that, params string[] names)
    {
        foreach (string name in names)
        {
            that.AddRecipient(name: name);
        }
    }
}

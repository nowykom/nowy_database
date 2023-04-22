using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public interface INowyMessageHub
{
    Task BroadcastMessageAsync(string category, object[] values, NowyMessageOptions? options);
    Task WaitUntilConnectedAsync(string event_name, CancellationToken token);
    Task WaitUntilConnectedAsync(string event_name, TimeSpan delay);
}

public class NowyMessageOptions
{
    public bool ExceptSender { get; set; }
}

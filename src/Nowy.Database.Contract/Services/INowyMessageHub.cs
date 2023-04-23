using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public interface INowyMessageHub
{
    Task BroadcastMessageAsync(string event_name, object[] values, NowyMessageOptions? options);
    Task WaitUntilConnectedAsync(string event_name, CancellationToken token);
    Task WaitUntilConnectedAsync(string event_name, TimeSpan delay);
}

public readonly record struct NowyMessageOptions(bool ExceptSender);

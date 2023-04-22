using Nowy.Database.Contract.Models;

namespace Nowy.Database.Contract.Services;

public interface INowyMessageHub
{
    Task BroadcastMessageAsync<TValue>(string category, TValue value) where TValue : class;
    Task WaitUntilConnectedAsync(string event_name, CancellationToken token);
    Task WaitUntilConnectedAsync(string event_name, TimeSpan delay);
}

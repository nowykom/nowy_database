using System.Text.Json;
using Nowy.Database.Contract.Services;
using SocketIOClient;

namespace Nowy.Database.Common.Services;

internal class DefaultNowyMessageHub : INowyMessageHub
{
    private readonly SocketIOService _socket_io_service;

    public DefaultNowyMessageHub(SocketIOService socket_io_service)
    {
        _socket_io_service = socket_io_service;
    }

    public async Task BroadcastMessageAsync(string event_name, object[] values, NowyMessageOptions? options)
    {
        await _socket_io_service.BroadcastMessageAsync(event_name, values, options);
    }

    public async Task WaitUntilConnectedAsync(string event_name, CancellationToken token)
    {
        await _socket_io_service.WaitUntilConnectedAsync(event_name, token);
    }

    public async Task WaitUntilConnectedAsync(string event_name, TimeSpan delay)
    {
        await _socket_io_service.WaitUntilConnectedAsync(event_name, delay);
    }
}

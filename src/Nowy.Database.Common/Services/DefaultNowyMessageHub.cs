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

    public async Task BroadcastMessageAsync<TValue>(string category, TValue value) where TValue : class
    {
        await _socket_io_service.BroadcastMessageAsync(category, value);
    }
}

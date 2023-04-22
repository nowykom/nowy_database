using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nowy.Database.Contract.Services;
using SocketIOClient;

namespace Nowy.Database.Common.Services;

internal sealed class SocketIOService : BackgroundService
{
    private static readonly JsonSerializerOptions _json_options = new JsonSerializerOptions() { PropertyNamingPolicy = null, };

    private readonly ILogger _logger;
    private readonly SocketIOConfig _config;
    private readonly List<INowyMessageHubReceiver> _receivers;
    private readonly List<EndpointEntry> _clients;

    public SocketIOService(ILogger<SocketIOService> logger, SocketIOConfig config, IEnumerable<INowyMessageHubReceiver> receivers)
    {
        _logger = logger;
        _config = config;
        _receivers = receivers.ToList();

        List<EndpointEntry> clients = new();
        foreach (NowyMessageHubEndpointConfig endpoint_config in config.Endpoints)
        {
            SocketIO client = new SocketIO(endpoint_config.Url, new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = int.MaxValue,
                ReconnectionDelay = 5000,
                RandomizationFactor = 0.5,
                ConnectionTimeout = TimeSpan.FromSeconds(5),

                Query = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("token", "abc123"),
                    new KeyValuePair<string, string>("key", "value")
                }
            });

            // client.OnAny((a, b) => { _logger.LogInformation($"Received message from Socket IO: event_name={a}, b={JsonSerializer.Serialize(b)}"); });

            foreach (INowyMessageHubReceiver message_hub_receiver in _receivers)
            {
                client.On("v1:broadcast_event", response => _handleResponseAsync(response, message_hub_receiver).Forget());
            }

            clients.Add(new EndpointEntry(client, endpoint_config));
        }

        _clients = clients;
    }

    private async Task _handleResponseAsync(SocketIOResponse response, INowyMessageHubReceiver receiver)
    {
        string event_name = response.GetValue<string>(0);
        _logger.LogInformation($"Received message from Socket IO: event_name={JsonSerializer.Serialize(event_name)}");

        bool matches = false;
        foreach (string event_name_prefix in receiver.GetEventNamePrefixes())
        {
            if (event_name.StartsWith(event_name_prefix))
            {
                matches = true;
                break;
            }
        }

        if (!matches)
            return;

        int values_count = response.GetValue<int>(1);
        List<string> values_as_json = new();

        for (int i = 0; i < values_count; i++)
        {
            string value_as_json = response.GetValue<string>(2 + i);
            values_as_json.Add(value_as_json);
        }

        _logger.LogInformation($"Received message from Socket IO: {JsonSerializer.Serialize(values_as_json)}");

        _Payload payload = new(values_as_json, _json_options);

        await receiver.ReceiveMessageAsync(event_name, payload);
    }

    public async Task WaitUntilConnectedAsync(string event_name, TimeSpan delay)
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(delay);
        await WaitUntilConnectedAsync(event_name, cts.Token);
    }

    public async Task WaitUntilConnectedAsync(string event_name, CancellationToken token)
    {
        List<EndpointEntry> matching_endpoint_entries = new();
        foreach (EndpointEntry client_entry in this._clients)
        {
            if (client_entry.EndpointConfig.IsEventNameAllowed(event_name))
            {
                matching_endpoint_entries.Add(client_entry);
            }
        }

        while (!token.IsCancellationRequested)
        {
            bool is_connected = matching_endpoint_entries.Any(o => o.Client.Connected);
            if (is_connected)
            {
                break;
            }

            await Task.Delay(100);
        }
    }

    private bool _isConnected(string event_name)
    {
        bool? is_connected = null;

        foreach (EndpointEntry client_entry in this._clients)
        {
            if (client_entry.EndpointConfig.IsEventNameAllowed(event_name))
            {
                if (client_entry.Client.Connected)
                {
                    is_connected = true;
                }
            }
        }

        return is_connected ?? false;
    }

    public async Task BroadcastMessageAsync(string event_name, params object[] values)
    {
        List<object> data = new();
        data.Add(event_name);
        data.Add(values.Length);

        foreach (object o in values)
        {
            data.Add(o as string ?? JsonSerializer.Serialize(o, _json_options));
        }

        _logger.LogInformation($"Send message to Socket IO: {JsonSerializer.Serialize(data)}");

        await Task.WhenAll(this._clients.Select(async client_entry =>
        {
            NowyMessageHubEndpointConfig endpoint_config = client_entry.EndpointConfig;
            if (endpoint_config.IsEventNameAllowed(event_name))
            {
                SocketIO client = client_entry.Client;
                if (!client.Connected)
                    throw new InvalidOperationException($"Endpoint '{client_entry.EndpointConfig.Url}' is not connected.");

                await client.EmitAsync(eventName: "v1:broadcast_event", data: data.ToArray());
            }
        }));
    }

    internal sealed class _Payload : INowyMessageHubReceiverPayload
    {
        private readonly List<string> _values_as_json;
        private readonly JsonSerializerOptions _json_options;

        private int _next_index = 0;

        public _Payload(List<string> values_as_json, JsonSerializerOptions json_options)
        {
            _values_as_json = values_as_json;
            _json_options = json_options;
        }


        public TValue? GetValue<TValue>() where TValue : class
        {
            int index = _next_index;
            TValue? value = GetValue<TValue>(index);
            _next_index++;
            return value;
        }

        public TValue? GetValue<TValue>(int index) where TValue : class
        {
            string str = _values_as_json[index];

            if (typeof(TValue) == typeof(string))
            {
                return str as TValue;
            }

            return JsonSerializer.Deserialize<TValue>(str, _json_options);
        }
    }

    internal sealed class EndpointEntry
    {
        internal readonly SocketIO Client;
        internal readonly NowyMessageHubEndpointConfig EndpointConfig;

        public EndpointEntry(SocketIO client, NowyMessageHubEndpointConfig endpoint_config)
        {
            Client = client;
            EndpointConfig = endpoint_config;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await Task.WhenAll(this._clients.Select(async client_entry =>
        {
            await Task.Yield();

            SocketIO client = client_entry.Client;

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!client.Connected)
                    {
                        try
                        {
                            _logger.LogInformation($"Connect to Socket IO: {client_entry.EndpointConfig.Url}");
                            await client.ConnectAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Connect to Socket IO: Error during {nameof(client.ConnectAsync)}");

                            await Task.Delay(_config.ConnectionRetryDelay, stoppingToken);
                        }
                    }

                    await Task.Delay(_config.ConnectionRetryLoopDelay, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Connect to Socket IO: Error during {nameof(ExecuteAsync)}");
            }
        }).ToArray());
    }
}
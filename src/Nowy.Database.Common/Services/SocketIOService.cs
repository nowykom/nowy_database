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

            foreach (INowyMessageHubReceiver message_hub_receiver in _receivers)
            {
                foreach (string event_name in message_hub_receiver.GetEventNamePrefixes())
                {
                    client.On($"v1:broadcast_event:{event_name}", response => _handleResponseAsync(response, message_hub_receiver).Forget());
                }
            }

            clients.Add(new EndpointEntry(client, endpoint_config));
        }

        _clients = clients;
    }

    private async Task _handleResponseAsync(SocketIOResponse response, INowyMessageHubReceiver receiver)
    {
        _logger.LogInformation($"Received message from Socket IO: {123}");

        string event_name = response.GetValue<string>(0);
        int values_count = response.GetValue<int>(1);
        List<string> values_as_json = new();

        for (int i = 0; i < values_count; i++)
        {
            string value_as_json = response.GetValue<string>(2 + i);
            values_as_json.Add(value_as_json);
        }

        _Payload payload = new(values_as_json, _json_options);

        await receiver.ReceiveMessageAsync(event_name, payload);
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

                await client.EmitAsync(eventName: $"v1:broadcast_event:{event_name}", data: data);
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

using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nowy.Database.Common.Models.Messages;
using Nowy.Database.Contract.Services;

namespace Nowy.MessageHub.Client.Services;

public readonly record struct EventQueueKey(string Name, NowyMessageOptions MessageOptions);

public readonly record struct EventQueueEntry(object Value);

internal class DefaultNowyMessageHubEventQueue : BackgroundService, INowyMessageHubEventQueue
{
    private readonly ILogger<DefaultNowyMessageHubEventQueue> _logger;
    private readonly INowyMessageHub _message_hub;
    private readonly object _lock_entries = new();
    private Dictionary<EventQueueKey, HashSet<EventQueueEntry>> _entries = new();
    private Stopwatch? _entries_were_added_at;

    public DefaultNowyMessageHubEventQueue(ILogger<DefaultNowyMessageHubEventQueue> logger, INowyMessageHub message_hub)
    {
        _logger = logger;
        _message_hub = message_hub;
    }

    public void QueueBroadcastMessage(string event_name, object event_value, NowyMessageOptions message_options)
    {
        EventQueueKey key = new(Name: event_name, MessageOptions: message_options);
        EventQueueEntry entry = new(Value: event_value);
        lock (this._lock_entries)
        {
            if (this._entries.TryGetValue(key, out HashSet<EventQueueEntry>? list))
            {
                list.Add(entry);
            }
            else
            {
                list = new HashSet<EventQueueEntry> { entry, };
                this._entries[key] = list;
            }

            this._entries_were_added_at = Stopwatch.StartNew();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await Task.Run(async () =>
        {
            const int delay_milliseconds = 1000;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (this._entries_were_added_at is { } entries_were_added_at)
                    {
                        if (entries_were_added_at.ElapsedMilliseconds is { } elapsed_milliseconds and < delay_milliseconds)
                        {
                            await Task.Delay(delay_milliseconds - (int)elapsed_milliseconds, stoppingToken);
                        }
                        else
                        {
                            Dictionary<EventQueueKey, HashSet<EventQueueEntry>> entries_copied;
                            lock (this._lock_entries)
                            {
                                entries_copied = this._entries;
                                this._entries = new();
                            }

                            foreach (( EventQueueKey key, HashSet<EventQueueEntry> entries_with_key ) in entries_copied)
                            {
                                await this._message_hub.BroadcastMessageAsync(
                                    event_name: key.Name,
                                    values: entries_with_key.Select(o => o.Value).ToArray(),
                                    options: key.MessageOptions
                                );
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(delay_milliseconds, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                }
            }
        });
    }
}

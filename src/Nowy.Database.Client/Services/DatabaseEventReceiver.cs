using Microsoft.Extensions.Logging;
using Nowy.Database.Common.Models.Messages;
using Nowy.Database.Contract.Services;

namespace Nowy.Database.Client.Services;

public class DatabaseEventReceiver : INowyMessageHubReceiver
{
    private readonly ILogger _logger;
    private readonly IDatabaseEventService _event_service;
    private readonly string[] _event_name_prefixes;
    private readonly string _event_name_prefix_collection_changed_insert;
    private readonly string _event_name_prefix_collection_changed_update;
    private readonly string _event_name_prefix_collection_changed_delete;
    private readonly string _event_name_prefix_entity_changed;

    public DatabaseEventReceiver(ILogger<DatabaseEventReceiver> logger, IDatabaseEventService event_service)
    {
        _logger = logger;
        _event_service = event_service;
        _event_name_prefixes = new[]
        {
            DatabaseCollectionChangedMessage.GetName(),
            DatabaseEntityChangedMessage.GetName(),
        };
        _event_name_prefix_collection_changed_insert = DatabaseCollectionChangedMessage.GetName(DatabaseEntityChangedType.INSERT);
        _event_name_prefix_collection_changed_update = DatabaseCollectionChangedMessage.GetName(DatabaseEntityChangedType.UPDATE);
        _event_name_prefix_collection_changed_delete = DatabaseCollectionChangedMessage.GetName(DatabaseEntityChangedType.DELETE);
        _event_name_prefix_entity_changed = DatabaseEntityChangedMessage.GetName();
    }

    public IEnumerable<string> GetEventNamePrefixes()
    {
        return _event_name_prefixes;
    }

    public async Task ReceiveMessageAsync(string event_name, INowyMessageHubReceiverPayload payload)
    {
        await Task.Yield();

        this._logger.LogInformation("Received database event: {event_name}, {payload}", event_name, payload);

        if (event_name.StartsWith(this._event_name_prefix_collection_changed_insert, StringComparison.Ordinal))
        {
            for (int i = 0; i < payload.Count; i++)
            {
                DatabaseCollectionChangedMessage? message = payload.GetValue<DatabaseCollectionChangedMessage>(i);
                if (message is null) throw new ArgumentNullException(nameof(DatabaseCollectionChangedMessage));

                this._event_service.SendCollectionModelsInserted(new CollectionChangedEventArgs(
                    message.DatabaseName ?? throw new ArgumentNullException(nameof(message.DatabaseName)),
                    message.EntityName ?? throw new ArgumentNullException(nameof(message.EntityName))
                ));
            }
        }

        if (event_name.StartsWith(this._event_name_prefix_collection_changed_update, StringComparison.Ordinal))
        {
            for (int i = 0; i < payload.Count; i++)
            {
                DatabaseCollectionChangedMessage? message = payload.GetValue<DatabaseCollectionChangedMessage>(i);
                if (message is null) throw new ArgumentNullException(nameof(DatabaseCollectionChangedMessage));

                this._event_service.SendCollectionModelsUpdated(new CollectionChangedEventArgs(
                    message.DatabaseName ?? throw new ArgumentNullException(nameof(message.DatabaseName)),
                    message.EntityName ?? throw new ArgumentNullException(nameof(message.EntityName))
                ));
            }
        }

        if (event_name.StartsWith(this._event_name_prefix_collection_changed_delete, StringComparison.Ordinal))
        {
            for (int i = 0; i < payload.Count; i++)
            {
                DatabaseCollectionChangedMessage? message = payload.GetValue<DatabaseCollectionChangedMessage>(i);
                if (message is null) throw new ArgumentNullException(nameof(DatabaseCollectionChangedMessage));

                this._event_service.SendCollectionModelsDeleted(new CollectionChangedEventArgs(
                    message.DatabaseName ?? throw new ArgumentNullException(nameof(message.DatabaseName)),
                    message.EntityName ?? throw new ArgumentNullException(nameof(message.EntityName))
                ));
            }
        }
    }
}

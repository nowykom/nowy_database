using MongoDB.Bson;
using MongoDB.Driver;
using Nowy.Database.Common.Models.Messages;
using Nowy.Database.Contract.Services;
using Nowy.MessageHub.Client.Services;
using Nowy.Standard;

namespace Nowy.Database.Web.Services;

public class MongoRepository
{
    private readonly IMongoClient _mongo_client;
    private readonly INowyMessageHub _message_hub;
    private readonly INowyMessageHubEventQueue _message_hub_event_queue;

    public MongoRepository(IMongoClient mongo_client, INowyMessageHub message_hub, INowyMessageHubEventQueue message_hub_event_queue)
    {
        _mongo_client = mongo_client;
        _message_hub = message_hub;
        _message_hub_event_queue = message_hub_event_queue;
    }

    private void _convertFromTransfer(BsonDocument input)
    {
        if (input.Contains("_id"))
        {
            throw new ArgumentException($"Bson Convert from Transfer to Internal: document is not in transfer state: document must not contain '_id': {input}");
        }

        if (!input.Contains("id"))
        {
            throw new ArgumentException($"Bson Convert from Transfer to Internal: document is not in transfer state: document must contain 'id': {input}");
        }

        input["_id"] = input["id"];
        input.Remove("id");

        if (input.Contains("ids"))
        {
            input["_ids"] = input["ids"];
            input.Remove("ids");
        }
        else
        {
            input["_ids"] = new BsonArray();
        }
    }

    private void _convertIntoTransfer(BsonDocument document)
    {
        if (!document.Contains("_id"))
        {
            throw new ArgumentException($"Bson Convert from Internal to Transfer: document is not in internal state: document must contain '_id': {document}");
        }

        document.Remove("id"); // always discard

        document["id"] = document["_id"];
        document.Remove("_id");

        if (document.Contains("_ids"))
        {
            document["ids"] = document["_ids"];
            document.Remove("_ids");
        }
        else
        {
            document["ids"] = new BsonArray();
        }
    }

    public async Task<List<BsonDocument>> GetAllAsync(string database_name, string entity_name)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        List<BsonDocument> list = await collection.Find(_ => true).ToListAsync();

        foreach (BsonDocument document in list)
        {
            _convertIntoTransfer(document);
        }

        return list;
    }

    public async Task<BsonDocument?> GetByIdAsync(string database_name, string entity_name, string id)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            Builders<BsonDocument>.Filter.In("_ids", new[] { id, })
        );

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();

        if (document is { })
        {
            _convertIntoTransfer(document);
        }

        return document;
    }

    public async Task<BsonDocument> UpsertAsync(string database_name, string entity_name, string id, BsonDocument input)
    {
        bool disable_events = false;
        if (input.Contains("disable_events"))
        {
            disable_events = input["disable_events"].AsBoolean;
            input.Remove("disable_events");
        }

        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        input["id"] = id;
        this._convertFromTransfer(input);

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            Builders<BsonDocument>.Filter.In("_ids", new[] { id, })
        );

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is not null)
        {
            await collection.UpdateOneAsync(filter, new BsonDocument
            {
                ["$set"] = input,
            });
        }
        else
        {
            await collection.InsertOneAsync(input);
        }

        document = await collection.Find(filter).FirstOrDefaultAsync();

        if (!disable_events)
        {
            this._message_hub_event_queue.QueueBroadcastMessage(
                DatabaseEntityChangedMessage.GetName(database_name: database_name, entity_name: entity_name),
                new DatabaseEntityChangedMessage()
                {
                    DatabaseName = database_name,
                    EntityName = entity_name,
                    Id = id,
                },
                new NowyMessageOptions() { ExceptSender = true, }
            );
            this._message_hub_event_queue.QueueBroadcastMessage(
                DatabaseCollectionChangedMessage.GetName(database_name: database_name, entity_name: entity_name),
                new DatabaseCollectionChangedMessage()
                {
                    DatabaseName = database_name,
                    EntityName = entity_name,
                },
                new NowyMessageOptions() { ExceptSender = true, }
            );
        }

        if (document is { })
        {
            _convertIntoTransfer(document);
        }

        return document;
    }

    public async Task<BsonDocument?> DeleteAsync(string database_name, string entity_name, string id)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            Builders<BsonDocument>.Filter.In("_ids", new[] { id, })
        );

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is not null)
        {
            await collection.DeleteOneAsync(filter);
        }

        return document;
    }
}

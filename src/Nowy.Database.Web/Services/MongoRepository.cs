using MongoDB.Bson;
using MongoDB.Driver;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;
using Nowy.MessageHub.Client.Services;
using Nowy.Standard;

namespace Nowy.Database.Web.Services;

public class MongoRepository
{
    private readonly ILogger _logger;
    private readonly IMongoClient _mongo_client;
    private readonly INowyMessageHub _message_hub;

    public MongoRepository(ILogger<MongoRepository> logger, IMongoClient mongo_client, INowyMessageHub message_hub)
    {
        _logger = logger;
        _mongo_client = mongo_client;
        _message_hub = message_hub;
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

    public async Task<List<BsonDocument>> GetByFilterAsync(string database_name, string entity_name, ModelFilter filter)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        FilterDefinition<BsonDocument> filter_bson = filter.ToMongoFilter();
        _logger.LogInformation(
            "Filter: {filter_bson}",
            filter_bson.Render(collection.DocumentSerializer, collection.Settings.SerializerRegistry).ToString()
        );

        List<BsonDocument> list = await collection.Find(filter_bson).ToListAsync();

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

        List<FilterDefinition<BsonDocument>> filters_or = new();
        filters_or.Add(Builders<BsonDocument>.Filter.Eq("_id", id));
        filters_or.Add(Builders<BsonDocument>.Filter.In("_ids", new[] { id, }));
        foreach (string id2 in input["_ids"].AsBsonArray.Values.Select(o => o.AsString))
        {
            filters_or.Add(Builders<BsonDocument>.Filter.Eq("_id", id2));
            filters_or.Add(Builders<BsonDocument>.Filter.In("_ids", new[] { id2, }));
        }

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(filters_or);

        bool is_inserted = false;
        bool is_updated = false;

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is not null)
        {
            BsonDocument input_clone = (BsonDocument)input.DeepClone();
            input_clone.Remove("_id");
            await collection.UpdateOneAsync(filter, new BsonDocument
            {
                ["$set"] = input_clone,
            });

            is_updated = true;
        }
        else
        {
            await collection.InsertOneAsync(input);

            is_inserted = true;
        }

        document = await collection.Find(filter).FirstOrDefaultAsync();

        if (!disable_events)
        {
            if (is_inserted)
            {
                CollectionEvent collection_event = new CollectionModelsInsertedEvent { DatabaseName = database_name, EntityName = entity_name, };
                CollectionModelEvent collection_model_event = new CollectionModelInsertedEvent { DatabaseName = database_name, EntityName = entity_name, Id = id, };
                CollectionChangedEvent collection_changed_event = new CollectionChangedEvent { DatabaseName = database_name, EntityName = entity_name, };

                this._message_hub.QueueEvent(config =>
                {
                    config.AddValue(collection_event);
                    config.AddValue(collection_model_event);
                    config.AddValue(collection_changed_event);
                });
            }

            if (is_updated)
            {
                CollectionEvent collection_event = new CollectionModelsUpdatedEvent { DatabaseName = database_name, EntityName = entity_name, };
                CollectionModelEvent collection_model_event = new CollectionModelUpdatedEvent { DatabaseName = database_name, EntityName = entity_name, Id = id, };
                CollectionChangedEvent collection_changed_event = new CollectionChangedEvent { DatabaseName = database_name, EntityName = entity_name, };

                this._message_hub.QueueEvent(config =>
                {
                    config.AddValue(collection_event);
                    config.AddValue(collection_model_event);
                    config.AddValue(collection_changed_event);
                });
            }
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

        bool is_deleted = false;

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is not null)
        {
            await collection.DeleteOneAsync(filter);

            is_deleted = true;
        }

        if (is_deleted)
        {
            CollectionEvent collection_event = new CollectionModelsDeletedEvent { DatabaseName = database_name, EntityName = entity_name, };
            CollectionModelEvent collection_model_event = new CollectionModelDeletedEvent { DatabaseName = database_name, EntityName = entity_name, Id = id, };
            CollectionChangedEvent collection_changed_event = new CollectionChangedEvent { DatabaseName = database_name, EntityName = entity_name, };

            this._message_hub.QueueEvent(config =>
            {
                config.AddValue(collection_event);
                config.AddValue(collection_model_event);
                config.AddValue(collection_changed_event);
            });
        }

        return document;
    }
}

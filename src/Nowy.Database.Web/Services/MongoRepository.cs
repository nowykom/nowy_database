using MongoDB.Bson;
using MongoDB.Driver;

namespace Nowy.Database.Web.Services;

public class MongoRepository
{
    private readonly IMongoClient _mongo_client;

    public MongoRepository(IMongoClient mongo_client)
    {
        _mongo_client = mongo_client;
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
            Builders<BsonDocument>.Filter.In("_ids", id)
        );

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();

        if (document is { })
        {
            _convertIntoTransfer(document);
        }

        return document;
    }

    public async Task<BsonDocument> CreateAsync(string database_name, string entity_name, BsonDocument input)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        if (!input.TryGetValue("id", out BsonValue v))
        {
            // input["id"] = StringHelper.MakeRandomUuid();
            throw new ArgumentNullException("id");
        }

        string id = (string)input["id"];
        input["_id"] = id;
        input.Remove("id");

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            Builders<BsonDocument>.Filter.In("_ids", id)
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

        if (document is { })
        {
            _convertIntoTransfer(document);
        }

        return document;
    }

    public async Task<BsonDocument> UpsertAsync(string database_name, string entity_name, string id, BsonDocument input)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        input["id"] = id;
        this._convertFromTransfer(input);

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Or(
            Builders<BsonDocument>.Filter.Eq("_id", id),
            Builders<BsonDocument>.Filter.In("_ids", id)
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
            Builders<BsonDocument>.Filter.In("_ids", id)
        );

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is not null)
        {
            await collection.DeleteOneAsync(filter);
        }

        return document;
    }
}

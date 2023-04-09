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

    public async Task<List<BsonDocument>> GetAllAsync(string database_name, string entity_name)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        List<BsonDocument> list = await collection.Find(_ => true).ToListAsync();

        foreach (BsonDocument document in list)
        {
            document["uuid"] = document["_id"];
            document.Remove("_id");
        }

        return list;
    }

    public async Task<BsonDocument?> GetByIdAsync(string database_name, string entity_name, string uuid)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", uuid);

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();

        if (document is { })
        {
            document["uuid"] = document["_id"];
            document.Remove("_id");
        }

        return document;
    }

    public async Task<BsonDocument> CreateAsync(string database_name, string entity_name, BsonDocument input)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        if (!input.TryGetValue("uuid", out BsonValue v))
        {
            // input["uuid"] = StringHelper.MakeRandomUuid();
            throw new ArgumentNullException("uuid");
        }

        string uuid = (string)input["uuid"];
        input["_id"] = uuid;
        input.Remove("uuid");

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", uuid);

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
            document["uuid"] = document["_id"];
            document.Remove("_id");
        }

        return document;
    }

    public async Task<BsonDocument> UpsertAsync(string database_name, string entity_name, string uuid, BsonDocument input)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        input["_id"] = uuid;
        input.Remove("uuid");

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", uuid);

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
            document["uuid"] = document["_id"];
            document.Remove("_id");
        }

        return document;
    }

    public async Task<BsonDocument?> DeleteAsync(string database_name, string entity_name, string uuid)
    {
        IMongoDatabase database = _mongo_client.GetDatabase(database_name) ?? throw new ArgumentNullException(nameof(database));
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(entity_name) ?? throw new ArgumentNullException(nameof(collection));

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", uuid);

        BsonDocument? document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is not null)
        {
            await collection.DeleteOneAsync(filter);
        }

        return document;
    }
}

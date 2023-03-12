using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Nowy.Database.Web.Services;

public static class MongoExtensions
{
    private static readonly JsonWriterSettings _json_writer_settings = new JsonWriterSettings() { OutputMode = JsonOutputMode.RelaxedExtendedJson, };

    public static async Task<MemoryStream> MongoToJsonStream(this IReadOnlyList<BsonDocument> list)
    {
        MemoryStream stream = new();

        await using StreamWriter writer = new(stream, leaveOpen: true);
        await writer.WriteAsync("[");
        await writer.FlushAsync();
        string sep = "";

        IBsonSerializer? serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
        foreach (BsonDocument document in list)
        {
            await writer.WriteAsync(sep);
            await writer.FlushAsync();
            sep = ",";

            using (JsonWriter bsonWriter = new JsonWriter(writer, _json_writer_settings))
            {
                BsonSerializationContext? context = BsonSerializationContext.CreateRoot(bsonWriter);
                serializer.Serialize(context, args: default, document);
            }

            await writer.FlushAsync();
        }

        await writer.WriteAsync("]");
        await writer.FlushAsync();

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public static async Task<MemoryStream> MongoToJsonStream(this BsonDocument document)
    {
        MemoryStream stream = new();

        await using (StreamWriter writer = new(stream, leaveOpen: true))
        {
            IBsonSerializer? serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));

            using JsonWriter bsonWriter = new JsonWriter(writer, _json_writer_settings);
            BsonSerializationContext? context = BsonSerializationContext.CreateRoot(bsonWriter);
            serializer.Serialize(context, args: default, document);
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}

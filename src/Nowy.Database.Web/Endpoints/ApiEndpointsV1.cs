using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Nowy.Database.Web.Services;

namespace Nowy.Database.Web.Endpoints;

public class ApiEndpointsV1
{
    private readonly ILogger _logger;

    public ApiEndpointsV1(ILogger<ApiEndpointsV1> logger)
    {
        _logger = logger;
    }

    public void MapEndpoints(RouteGroupBuilder app)
    {
        app.MapGet("{database_name}/{entity_name}", GetAllAsync);
        app.MapGet("{database_name}/{entity_name}/{uuid}", GetByIdAsync);
        app.MapPost("{database_name}/{entity_name}/{uuid}", UpdateAsync);
        app.MapPut("{database_name}/{entity_name}/{uuid}", UpdateAsync);
        app.MapDelete("{database_name}/{entity_name}/{uuid}", DeleteAsync);
    }

    public async Task<Results<FileContentHttpResult, NotFound>> GetAllAsync(MongoRepository repo, string database_name, string entity_name)
    {
        _logger.LogInformation($"{nameof(GetAllAsync)}: {nameof(database_name)} = {database_name}, {nameof(entity_name)} = {entity_name}");

        List<BsonDocument> list = await repo.GetAllAsync(database_name: database_name, entity_name: entity_name);

        await using MemoryStream stream = await list.MongoToJsonStream();
        return TypedResults.Bytes(stream.ToArray(), "application/json");
    }

    public async Task<Results<FileContentHttpResult, NoContent, NotFound>> GetByIdAsync(MongoRepository repo, string database_name, string entity_name, string uuid)
    {
        _logger.LogInformation($"{nameof(GetByIdAsync)}: {nameof(database_name)} = {database_name}, {nameof(entity_name)} = {entity_name}, {nameof(uuid)} = {uuid}");

        BsonDocument? document = await repo.GetByIdAsync(database_name: database_name, entity_name: entity_name, uuid: uuid);

        if (document is { })
        {
            await using MemoryStream stream = await document.MongoToJsonStream();
            return TypedResults.Bytes(stream.ToArray(), "application/json");
        }
        else
        {
            return TypedResults.NoContent();
        }
    }

    public async Task<Results<FileContentHttpResult, NotFound>> UpdateAsync(MongoRepository repo, string database_name, string entity_name, string uuid,
        HttpRequest request)
    {
        string input_json;
        await using (MemoryStream input_json_stream = new MemoryStream(2048))
        {
            await request.Body.CopyToAsync(input_json_stream);
            input_json = Encoding.UTF8.GetString(input_json_stream.ToArray());
        }

        _logger.LogInformation(
            $"{nameof(UpdateAsync)}: {nameof(database_name)} = {database_name}, {nameof(entity_name)} = {entity_name}, {nameof(uuid)} = {uuid}, {nameof(input_json)} = {input_json}");

        BsonDocument input_document = BsonDocument.Parse(input_json);
        BsonDocument? document = await repo.UpsertAsync(database_name: database_name, entity_name: entity_name, uuid: uuid, input: input_document);
        await using MemoryStream stream = await document.MongoToJsonStream();

        _logger.LogInformation($"{nameof(UpdateAsync)}: => {Encoding.UTF8.GetString(stream.ToArray())}");

        return TypedResults.Bytes(stream.ToArray(), "application/json");
    }

    public async Task<Results<FileContentHttpResult, NoContent, NotFound>> DeleteAsync(MongoRepository repo, string database_name, string entity_name, string uuid)
    {
        _logger.LogInformation($"{nameof(DeleteAsync)}: {nameof(database_name)} = {database_name}, {nameof(entity_name)} = {entity_name}, {nameof(uuid)} = {uuid}");

        BsonDocument? document = await repo.DeleteAsync(database_name: database_name, entity_name: entity_name, uuid: uuid);

        if (document is { })
        {
            await using MemoryStream stream = await document.MongoToJsonStream();
            return TypedResults.Bytes(stream.ToArray(), "application/json");
        }
        else
        {
            return TypedResults.NoContent();
        }
    }
}

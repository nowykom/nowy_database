using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Nowy.Database.Web.Endpoints;
using Nowy.Database.Web.Services;
using Nowy.Standard;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

BsonSerializer.RegisterIdGenerator(typeof(string), new StringObjectIdGenerator());
BsonSerializer.RegisterSerializer(typeof(UnixTimestamp), new MongoUnixTimestampSerializer());

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb"))
);
builder.Services.AddSingleton<MongoRepository>(sp =>
    new MongoRepository(sp.GetRequiredService<IMongoClient>())
);

builder.Services.AddSingleton<ApiEndpointsV1>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // app.UseExceptionHandler("/Error");
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nowy Database"); });


app.Services.GetRequiredService<ApiEndpointsV1>().MapEndpoints(app.MapGroup("api/v1"));

app.Run();

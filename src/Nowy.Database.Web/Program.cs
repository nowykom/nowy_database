using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Nowy.Auth.Server;
using Nowy.Database.Contract.Models;
using Nowy.Database.Contract.Services;
using Nowy.Database.Web.Endpoints;
using Nowy.Database.Web.Services;
using Nowy.MessageHub.Client;
using Nowy.Standard;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

BsonSerializer.RegisterIdGenerator(typeof(string), new StringObjectIdGenerator());
BsonSerializer.RegisterSerializer(typeof(UnixTimestamp), new MongoUnixTimestampSerializer());

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb"))
);
builder.Services.AddSingleton<MongoRepository>();
builder.Services.AddSingleton<INowyDatabase, MongoNowyDatabase>();

builder.Services.AddNowyMessageHubClient(config =>
{
    config.AddEndpoint("https://main.messagehub.schulz.dev");
    config.AddEndpoint("https://main.messagehub.nowykom.de");
});

builder.Services.AddSingleton<ApiEndpointsV1>();

builder.Services.AddNowyStandard();
builder.Services.AddNowyAuthServer();

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

app.UseNowyAuthServer();

app.Services.GetRequiredService<ApiEndpointsV1>().MapEndpoints(app.MapGroup("api/v1"));

app.Run();

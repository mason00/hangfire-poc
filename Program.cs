using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var mongoUrlBuilder = new MongoUrlBuilder("mongodb+srv://dbUser:lhDSzFEUV0xNWKix@cluster0.2tuja.azure.mongodb.net/") { DatabaseName = "jobs" };
var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

var db = mongoClient.GetDatabase("Jobs");

// Add Hangfire services. Hangfire.AspNetCore nuget required
services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMongoStorage(mongoClient, mongoUrlBuilder.DatabaseName, new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        },
        Prefix = "hangfire.mongo",
        CheckConnection = false
    })
);
// Add the processing server as IHostedService
services.AddHangfireServer(serverOptions =>
{
    serverOptions.ServerName = "Hangfire.Mongo server 1";
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseHangfireDashboard();
app.Run();

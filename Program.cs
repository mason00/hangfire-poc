using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

AddHangfire();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!").WithOpenApi();

app.MapGet("/createJob", () => {
    for (int i = 0; i < 30; i++)
    {
        Task.Run(() => BackgroundJob.Enqueue(() => Console.WriteLine($"Hello, {DateTime.Now}")));
    }
}).WithOpenApi();
app.MapGet("/scheduleJob", () => {
    for (int i = 0; i < 30; i++) {
        BackgroundJob.Schedule(() => Console.WriteLine($"scheduleJob, {DateTime.Now}"), DateTime.Now.AddSeconds(1));
    }
}).WithOpenApi();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [ new HangfireBasicAuthenticationFilter.HangfireCustomBasicAuthenticationFilter
        { User = "Mason", Pass = "nopass" } ]
});

app.Run();

void AddHangfire()
{
    var services = builder.Services;

    var mongoConn = builder.Configuration["mongoConnection"];
    var mongoUrlBuilder = new MongoUrlBuilder(mongoConn) { DatabaseName = "jobs" };
    var mongoClient = new MongoClient(mongoUrlBuilder.ToMongoUrl());

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
            CheckConnection = false,
        })
    );
    // Add the processing server as IHostedService
    services.AddHangfireServer(serverOptions =>
    {
        serverOptions.ServerName = "Hangfire.Mongo server 1";
    });
}

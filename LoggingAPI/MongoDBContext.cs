using MongoDB.Driver;
using LoggingAPI.Models;
using System;

public class MongoDbContext
{
    public IMongoCollection<LoggingEntry> Logs { get; }

    public MongoDbContext(IConfiguration configuration)
    {
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var mongoDbConnectionString = configuration.GetValue<string>("MONGODB_CONNECTION_STRING");
        if (environment == "Development")
        {
            // In Development, use the connection string from appsettings.json
            mongoDbConnectionString = configuration.GetConnectionString("MongoDBConnection");
        }
        else
        {
            // In non-Development, use the environment variable
            mongoDbConnectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "mongodb_connection_string";
        }
        var client = new MongoClient(configuration.GetConnectionString("MongoDBConnection"));
        var database = client.GetDatabase("logs");

        Logs = database.GetCollection<LoggingEntry>("logs");
    }
}

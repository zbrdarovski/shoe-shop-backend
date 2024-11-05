// DeliveryDbContext.cs
using MongoDB.Driver;

public class DeliveryDbContext
{
    public IMongoCollection<Delivery> Deliveries { get; }

    public DeliveryDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        Deliveries = database.GetCollection<Delivery>("Deliveries");
    }
}
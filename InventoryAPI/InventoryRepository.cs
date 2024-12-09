using InventoryAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InventoryAPI
{
    public class InventoryRepository
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;

        public InventoryRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("inventory");
            _inventoryCollection = database.GetCollection<Inventory>("inventory");
        }

        public async Task<List<Inventory>> GetAllItemsAsync()
        {
            return await _inventoryCollection.Find(item => true).ToListAsync();
        }

        public async Task<Inventory> GetItemByIdAsync(string itemId)
        {
            return await _inventoryCollection.Find(i => i.Id == itemId).FirstOrDefaultAsync();
        }

        public async Task AddItemAsync(Inventory item)
        {
            await _inventoryCollection.InsertOneAsync(item);
        }

        public async Task UpdateItemAsync(Inventory item)
        {
            await _inventoryCollection.ReplaceOneAsync(i => i.Id == item.Id, item);
        }

        public async Task DeleteItemAsync(string itemId)
        {
            await _inventoryCollection.DeleteOneAsync(item => item.Id == itemId);
        }

        public async Task AddQuantityAsync(string itemId, int quantityToAdd)
        {
            await _inventoryCollection.UpdateOneAsync(
                i => i.Id == itemId,
                Builders<Inventory>.Update.Inc(i => i.Quantity, quantityToAdd)
            );
        }

        public async Task SubtractQuantityAsync(string itemId, int quantityToSubtract)
        {
            await _inventoryCollection.UpdateOneAsync(
                i => i.Id == itemId && i.Quantity >= quantityToSubtract,
                Builders<Inventory>.Update.Inc(i => i.Quantity, -quantityToSubtract)
            );
        }

        public async Task ChangePriceAsync(string itemId, double newPrice)
        {
            await _inventoryCollection.UpdateOneAsync(
                i => i.Id == itemId,
                Builders<Inventory>.Update.Set(i => i.Price, newPrice)
            );
        }

        public async Task<List<Comment>> GetCommentsAsync(string itemId)
        {
            var item = await _inventoryCollection.Find(i => i.Id == itemId).FirstOrDefaultAsync();
            return item?.Comments ?? new List<Comment>();
        }

        public async Task<List<Rating>> GetRatingsAsync(string itemId)
        {
            var item = await _inventoryCollection.Find(i => i.Id == itemId).FirstOrDefaultAsync();
            return item?.Ratings ?? new List<Rating>();
        }
    }
}

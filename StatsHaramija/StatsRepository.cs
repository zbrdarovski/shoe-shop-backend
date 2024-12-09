using MongoDB.Driver;
using StatsHaramija.Models;

namespace StatsHaramija
{
    public class StatsRepository
    {
        private readonly IMongoCollection<ApiCallStat> _statsCollection;

        public StatsRepository(string connectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("statsharamija");
            _statsCollection = database.GetCollection<ApiCallStat>("statsharamija");
        }

        /*
        public async Task UpdateStatAsync(string endpoint)
        {
            var filter = Builders<ApiCallStat>.Filter.Eq(stat => stat.Endpoint, endpoint);
            var update = Builders<ApiCallStat>.Update
                .Inc(stat => stat.Count, 1)
                .Set(stat => stat.LastCalled, DateTime.UtcNow);
            var options = new UpdateOptions { IsUpsert = true };
            await _statsCollection.UpdateOneAsync(filter, update, options);
        }
        */

        public async Task UpdateStatAsync(string endpoint)
        {
            var filter = Builders<ApiCallStat>.Filter.Eq(stat => stat.Endpoint, endpoint);
            var update = Builders<ApiCallStat>.Update
                .Inc(stat => stat.Count, 1) // Inkrementacija števca
                .Set(stat => stat.LastCalled, DateTime.Now); // Posodobitev časa zadnjega klica
            var options = new UpdateOptions { IsUpsert = true };
            await _statsCollection.UpdateOneAsync(filter, update, options);
        }

        public async Task<ApiCallStat> GetLastCalledAsync()
        {
            return await _statsCollection.Find(_ => true).SortByDescending(stat => stat.LastCalled).FirstOrDefaultAsync();
        }

        public async Task<ApiCallStat> GetMostCalledAsync()
        {
            return await _statsCollection.Find(_ => true).SortByDescending(stat => stat.Count).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ApiCallStat>> GetAllStatsAsync()
        {
            return await _statsCollection.Find(_ => true).ToListAsync();
        }


    }
}

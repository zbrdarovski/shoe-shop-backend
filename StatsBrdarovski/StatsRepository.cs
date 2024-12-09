using MongoDB.Driver;
using StatsBrdarovski.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class StatsRepository
{
    private readonly IMongoCollection<ApiCallStat> _collection;

    public StatsRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ApiCallStat>("ApiCallStats");
    }

    public async Task UpdateStatAsync(string endpoint)
    {
        var filter = Builders<ApiCallStat>.Filter.Eq(s => s.Endpoint, endpoint);
        var update = Builders<ApiCallStat>.Update.Inc(s => s.CallCount, 1);
        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<ApiCallStat> GetLastCalledAsync()
    {
        return await _collection.Find(FilterDefinition<ApiCallStat>.Empty)
            .SortByDescending(s => s.LastCalled)
            .FirstOrDefaultAsync();
    }

    public async Task<ApiCallStat> GetMostCalledAsync()
    {
        return await _collection.Find(FilterDefinition<ApiCallStat>.Empty)
            .SortByDescending(s => s.CallCount)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ApiCallStat>> GetAllStatsAsync()
    {
        return await _collection.Find(FilterDefinition<ApiCallStat>.Empty)
            .ToListAsync();
    }
}

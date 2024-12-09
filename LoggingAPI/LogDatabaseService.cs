using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using LoggingAPI.Models;
using LoggingAPI;

public class LogDatabaseService
{
    private readonly IMongoCollection<LoggingEntry> _logsCollection;

    public LogDatabaseService(MongoDbContext dbContext)
    {
        _logsCollection = dbContext.Logs;
    }

    public async Task SaveLog(LoggingEntry logEntry)
    {
        await _logsCollection.InsertOneAsync(logEntry);
    }

    public async Task<List<LoggingEntry>> GetLogs(DateTime startDate, DateTime endDate)
    {
        return await _logsCollection.Find(log => log.Timestamp >= startDate && log.Timestamp <= endDate).ToListAsync();
    }

    public async Task DeleteAllLogs()
    {
        await _logsCollection.DeleteManyAsync(Builders<LoggingEntry>.Filter.Empty);
    }
}

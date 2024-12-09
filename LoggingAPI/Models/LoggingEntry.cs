using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace LoggingAPI.Models
{
    public class LoggingEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("logType")]
        public string LogType { get; set; }

        [BsonElement("url")]
        public string Url { get; set; }

        [BsonElement("correlationId")]
        public string CorrelationId { get; set; }

        [BsonElement("applicationName")]
        public string ApplicationName { get; set; }

        [BsonElement("message")]
        public string Message { get; set; }

    }
}

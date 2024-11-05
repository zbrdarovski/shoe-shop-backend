using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace StatsHaramija.Models
{
    public class ApiCallStat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Omogoča pretvorbo med ObjectId in string
        public string Id { get; set; }

        [BsonElement("Endpoint")]
        public string Endpoint { get; set; }

        [BsonElement("Count")]
        public int Count { get; set; }

        [BsonElement("LastCalled")]
        public DateTime LastCalled { get; set; }
    }

}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace StatsBrdarovski.Models
{
    public class ApiCallStat
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Endpoint { get; set; }
        public int CallCount { get; set; }
        public DateTime LastCalled { get; set; }
    }
}

using MongoDB.Bson.Serialization.Attributes;

namespace InventoryAPI.Models
{
    public class Rating
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("Value")]
        public int Value { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
    }
}

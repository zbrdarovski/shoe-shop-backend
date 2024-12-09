using MongoDB.Bson.Serialization.Attributes;

namespace InventoryAPI.Models
{
    public class Comment
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
    }
}

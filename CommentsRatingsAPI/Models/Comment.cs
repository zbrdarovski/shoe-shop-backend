using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CommentsRatingsAPI.Models
{
    public class Comment
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("ItemId")]
        public string ItemId { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }
    }
}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CartPaymentAPI.Models
{
    public class Cart
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("CartItems")]
        public List<InventoryItem>? CartItems { get; set; }

        [BsonElement("CartAmount")]
        public double? CartAmount { get; set; }
    }
}
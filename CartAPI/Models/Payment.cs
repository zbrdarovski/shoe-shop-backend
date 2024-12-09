using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CartPaymentAPI.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Specify that Id should be treated as a string
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("CartId")]
        public string? CartId { get; set; }

        [BsonElement("UserId")]
        public string? UserId { get; set; }

        [BsonElement("Amount")]
        public double Amount { get; set; }

        [BsonElement("InventoryItems")]
        public List<InventoryItem> InventoryItems { get; set; }

        [BsonElement("PaymentDate")]
        public DateTime PaymentDate { get; set; }
    }
}

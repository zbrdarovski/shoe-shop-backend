using MongoDB.Bson.Serialization.Attributes;

namespace CartPaymentAPI.Models
{
    public class InventoryItem
    {
        [BsonElement("Id")]
        public string Id { get; set; }

        [BsonElement("Price")]
        public double Price { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Color")]
        public string Color { get; set; }

        [BsonElement("Size")]
        public string Size { get; set; }

        [BsonElement("Description")]
        public string? Description { get; set; }

        [BsonElement("Image")]
        public byte[]? Image { get; set; }

        [BsonElement("Quantity")]
        public int? Quantity { get; set; }

    }
}

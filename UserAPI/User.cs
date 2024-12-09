// User.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonElement("Id")]
    public string? Id { get; set; }

    [BsonElement("Username")]
    public string? Username { get; set; }

    [BsonElement("Password")]
    public string? Password { get; set; }
}

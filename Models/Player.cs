using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Models;
public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("balance")]
    public double Balance { get; set; }

    [BsonElement("version")]
    public int Version { get; set; } = 1;
}
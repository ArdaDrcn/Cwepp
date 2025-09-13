using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CimaxWeb2.Models;

public class Device
{
    [BsonId] public ObjectId Id { get; set; }

    [BsonElement("ip")] public string? Ip { get; set; }
    [BsonElement("mac")] public string? Mac { get; set; }
    [BsonElement("status")] public string? Status { get; set; }
    [BsonElement("firmware")] public string? Firmware { get; set; }
    [BsonElement("model")] public string? Model { get; set; }
    [BsonElement("type")] public string? Type { get; set; }
    [BsonElement("location")] public string? Location { get; set; }
    [BsonElement("name")] public string? Name { get; set; }

    [BsonElement("createdAtUtc")] public DateTime? CreatedAtUtc { get; set; }
    [BsonElement("updatedAtUtc")] public DateTime? UpdatedAtUtc { get; set; }
}

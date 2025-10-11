using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CimaxWeb2.Models;

[BsonIgnoreExtraElements]
public class InterlockEvent
{
    [BsonId] public ObjectId Id { get; set; }

    [BsonElement("deviceIP")] public string? DeviceIP { get; set; }
    [BsonElement("mac")] public string? Mac { get; set; }

    // <-- Artık obje: { status: "0|1", value: "1928" }
    [BsonElement("door1")] public DoorChannel? Door1 { get; set; }
    [BsonElement("door2")] public DoorChannel? Door2 { get; set; }

    [BsonElement("createdAtUtc")] public DateTime? CreatedAtUtc { get; set; }
    [BsonElement("updatedAtUtc")] public DateTime? UpdatedAtUtc { get; set; }

    [BsonIgnore] public bool Door1Active => Door1?.Status == "1" || string.Equals(Door1?.Status, "true", StringComparison.OrdinalIgnoreCase);
    [BsonIgnore] public bool Door2Active => Door2?.Status == "1" || string.Equals(Door2?.Status, "true", StringComparison.OrdinalIgnoreCase);
    [BsonIgnore] public string Door1Value => Door1?.Value ?? "-";
    [BsonIgnore] public string Door2Value => Door2?.Value ?? "-";
}

public class DoorChannel
{
    [BsonElement("status")] public string? Status { get; set; } // "0" | "1" | "true"/"false"
    [BsonElement("value")] public string? Value { get; set; }  // "1928" gibi
}

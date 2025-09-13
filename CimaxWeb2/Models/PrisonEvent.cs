using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CimaxWeb2.Models;

[BsonIgnoreExtraElements]
public class PrisonEvent
{
    [BsonId] public ObjectId Id { get; set; }

    [BsonElement("deviceIP")] public string? DeviceIP { get; set; }
    [BsonElement("mac")] public string? Mac { get; set; }

    [BsonElement("emergencycall")] public int? EmergencyCall { get; set; } //örnek
    [BsonElement("door")] public int? Door { get; set; } //yeni
    [BsonElement("sound")] public int? Sound { get; set; } //yeni
    [BsonElement("intercom")] public int? Intercom { get; set; } //yeni
    [BsonElement("laser")] public int? Laser { get; set; } //yeni

    [BsonElement("coldwatermeter")] public WaterMeter? ColdWaterMeter { get; set; }
    [BsonElement("hotwatermeter")] public WaterMeter? HotWaterMeter { get; set; }

    [BsonElement("electricitymeter")] public ElectricityMeter? ElectricityMeter { get; set; }

    // ⚠️ Artık obje!
    [BsonElement("degree")] public DegreeSensor? Degree { get; set; }
    [BsonElement("humidity")] public HumiditySensor? Humidity { get; set; }

    [BsonElement("createdAtUtc")] public DateTime? CreatedAtUtc { get; set; }
    [BsonElement("updatedAtUtc")] public DateTime? UpdatedAtUtc { get; set; }
}

public class WaterMeter
{
    [BsonElement("status")] public string? Status { get; set; }
    [BsonElement("uploaded")] public string? Uploaded { get; set; }
    [BsonElement("consumed")] public string? Consumed { get; set; }
}

public class ElectricityMeter
{
    [BsonElement("status")] public string? Status { get; set; }
    [BsonElement("consumption")] public string? Consumption { get; set; }
}

public class DegreeSensor
{
    [BsonElement("status")] public string? Status { get; set; }
    [BsonElement("value")] public string? Value { get; set; }
}

public class HumiditySensor
{
    [BsonElement("status")] public string? Status { get; set; }
    [BsonElement("value")] public string? Value { get; set; }
}

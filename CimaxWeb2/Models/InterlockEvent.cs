using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CimaxWeb2.Models;

[BsonIgnoreExtraElements]
public class InterlockEvent
{
    [BsonId] public ObjectId Id { get; set; }

    [BsonElement("deviceIP")] public string? DeviceIP { get; set; }
    [BsonElement("mac")] public string? Mac { get; set; }

    // Kaynaktaki JSON'da "1"/"0" string olarak geliyor, bu yüzden string mapliyoruz.
    [BsonElement("door1")] public string? Door1 { get; set; }
    [BsonElement("door2")] public string? Door2 { get; set; }

    [BsonElement("createdAtUtc")] public DateTime? CreatedAtUtc { get; set; }
    [BsonElement("updatedAtUtc")] public DateTime? UpdatedAtUtc { get; set; }

    // Kullanımı kolaylaştıran yardımcı özellikler:
    [BsonIgnore] public bool Door1On => Door1 == "1";
    [BsonIgnore] public bool Door2On => Door2 == "1";
}

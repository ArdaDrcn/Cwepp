using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CimaxWeb2.Models;

public enum CardType
{
    Device,        // Ana cihaz
    ColdWater,     // Soğuk su sayacı
    HotWater,      // Sıcak su sayacı
    Electricity    // Elektrik sayacı
}

public class DeviceCardVM
{
    // örn: "192.168.1.103|device", "192.168.1.103|cold", "192.168.1.103|hot", "192.168.1.103|elec"
    public string CardKey { get; set; } = "";

    public CardType Type { get; set; }

    public Device Device { get; set; } = default!;
    public PrisonEvent? LatestEvent { get; set; }

    // 🔽 YENİ: Interlock son olayını da kart üzerinde taşıyabilelim
    public InterlockEvent? LatestInterlockEvent { get; set; }

    public string Title { get; set; } = "";
    public string IconPath { get; set; } = "/img/icon-0.png";
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using CimaxWeb2.Models;

namespace CimaxWeb2.Pages;

public class IndexModel : PageModel
{
    private readonly IMongoCollection<Device> _devices;
    private readonly IMongoCollection<PrisonEvent> _events;

    public List<DeviceCardVM> Cards { get; private set; } = new();

    public IndexModel(IMongoCollection<Device> devices, IMongoCollection<PrisonEvent> eventsCol)
    {
        _devices = devices;
        _events = eventsCol;
    }

    public async Task OnGet()
    {
        var deviceList = await _devices.Find(_ => true).Limit(200).ToListAsync();
        var ips = deviceList.Where(d => !string.IsNullOrWhiteSpace(d.Ip))
                            .Select(d => d.Ip!.Trim())
                            .Distinct()
                            .ToList();

        var events = ips.Count == 0
            ? new List<PrisonEvent>()
            : await _events.Find(e => ips.Contains((e.DeviceIP ?? string.Empty).Trim())).ToListAsync();

        var latestByIp = events
            .Where(e => !string.IsNullOrWhiteSpace(e.DeviceIP))
            .GroupBy(e => e.DeviceIP!.Trim())
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc ?? DateTime.MinValue).First()
            );

        var list = new List<DeviceCardVM>();

        foreach (var d in deviceList)
        {
            var ip = (d.Ip ?? string.Empty).Trim();
            latestByIp.TryGetValue(ip, out var evt);

            // 1) Ana cihaz kartý
            list.Add(new DeviceCardVM
            {
                CardKey = $"{ip}|device",
                Type = CardType.Device,
                Device = d,
                LatestEvent = evt,
                Title = (d.Name ?? "(isimsiz cihaz)") + (string.IsNullOrWhiteSpace(d.Location) ? "" : $" - {d.Location}"),
                IconPath = IconPathFromEmergency(evt)
            });

            // 2) Soðuk sayaç kartý (varsa)
            if (evt?.ColdWaterMeter != null)
            {
                list.Add(new DeviceCardVM
                {
                    CardKey = $"{ip}|cold",
                    Type = CardType.ColdWater,
                    Device = d,
                    LatestEvent = evt,
                    Title = $"Soðuk Su Sayacý - {d.Location}",
                    IconPath = IconPathForCold(evt.ColdWaterMeter)
                });
            }

            // 3) Sýcak sayaç kartý (varsa)
            if (evt?.HotWaterMeter != null)
            {
                list.Add(new DeviceCardVM
                {
                    CardKey = $"{ip}|hot",
                    Type = CardType.HotWater,
                    Device = d,
                    LatestEvent = evt,
                    Title = $"Sýcak Su Sayacý - {d.Location}",
                    IconPath = IconPathForHot(evt.HotWaterMeter)
                });
            }
        }

        Cards = list;
    }

    // --- Ýkon yollarý (PATH) ---
    public static string IconPathFromEmergency(PrisonEvent? evt)
    {
        var code = (evt?.EmergencyCall ?? 0) == 1 ? 1 : 0;
        return code == 1 ? "~/img/GardiyanAktifGif.gif" : "~/img/AcilCagriPasif.png";
    }

    public static string IconPathFromDoor(PrisonEvent? evt)
    {
        var code = (evt?.Door ?? 0) == 1 ? 1 : 0;
        return code == 1 ? "~/img/GirisKapiAktif.png" : "~/img/GirisKapiPasif.png";
    }

    // 0–5 mapping: dosya adlarý projendeki gerçek isimlerle eþleþmelidir
    public static string IconPathFromSound(PrisonEvent? evt)
    {
        var code = evt?.Sound ?? 0;
        return code switch
        {
            0 => "~/img/HoparlorKopuk.png",
            1 => "~/img/HoparlorPasif.png",
            2 => "~/img/HoparlorAktif1.png",
            3 => "~/img/HoparlorAktif2.png",
            4 => "~/img/HoparlorAktif3.png",
            5 => "~/img/HoparlorAktif4.png",
            _ => "~/img/HoparlorKopuk.png"
        };
    }

    public static string IconPathFromIntercom(PrisonEvent? evt)
    {
        var code = (evt?.Intercom ?? 0) == 1 ? 1 : 0;
        return code == 1 ? "~/img/InterkomAktif.png" : "~/img/InterkomPasif.png";
    }

    public static string IconPathFromLaser(PrisonEvent? evt)
    {
        var code = (evt?.Laser ?? 0) == 1 ? 1 : 0;
        return code == 1 ? "~/img/LazerAktif.png" : "~/img/LazerPasif.png";
    }

    private static string IconPathForCold(WaterMeter m)
    {
        var active = m.Status == "1" || m.Status?.ToLower() == "true";
        return active ? "~/img/SuSayacSogukAktif.png" : "~/img/SuSayacSogukPasif.png";
    }

    private static string IconPathForHot(WaterMeter m)
    {
        var active = m.Status == "1" || m.Status?.ToLower() == "true";
        return active ? "~/img/SuSayacSicakAktif.png" : "~/img/SuSayacSicakPasif.png";
    }

    private static string IconPathForElec(ElectricityMeter m)
    {
        var active = m.Status == "1" || m.Status?.ToLower() == "true";
        return active ? "~/img/ElektrikSayacAktif.png" : "~/img/ElektrikSayacPasif.png";
    }

    private static bool IsActive(string? s) =>
        s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);

    private static string IconPathForDegree(DegreeSensor m) =>
        IsActive(m.Status) ? "~/img/DereceAktif.png" : "~/img/DerecePasif.png";

    private static string IconPathForHumidity(HumiditySensor m) =>
        IsActive(m.Status) ? "~/img/NemAktif.png" : "~/img/NemPasif.png";

    // --- URL döndüren convenience yardýmcýlar (View'da kullanýyoruz) ---
    public string EmergencyIconUrl(PrisonEvent? evt) => Url.Content(IconPathFromEmergency(evt));
    public string DoorIconUrl(PrisonEvent? evt) => Url.Content(IconPathFromDoor(evt));
    public string SoundIconUrl(PrisonEvent? evt) => Url.Content(IconPathFromSound(evt));
    public string IntercomIconUrl(PrisonEvent? evt) => Url.Content(IconPathFromIntercom(evt));
    public string LaserIconUrl(PrisonEvent? evt) => Url.Content(IconPathFromLaser(evt));

    // --- PULSE DTO ---
    public record CardPulseDto(
        string Ip,
        long UpdatedAtTicks,
        string IconUrl,
        bool ShowOverlay,

        string? ColdIconUrl,
        string? ColdConsumed,

        string? HotIconUrl,
        string? HotConsumed,

        string? ElecIconUrl,
        string? ElecConsumption,

        string? DegreeIconUrl,
        string? DegreeValue,

        string? HumidityIconUrl,
        string? HumidityValue,

        string? LaserIconUrl,
        string? SoundIconUrl,
        string? IntercomIconUrl,
        string? DoorIconUrl
    );

    public async Task<IActionResult> OnGetPulse()
    {
        var deviceList = await _devices.Find(_ => true).Limit(200).ToListAsync();
        var ips = deviceList.Where(d => !string.IsNullOrWhiteSpace(d.Ip))
                            .Select(d => d.Ip!.Trim())
                            .Distinct()
                            .ToList();

        var events = ips.Count == 0
            ? new List<PrisonEvent>()
            : await _events.Find(e => ips.Contains((e.DeviceIP ?? string.Empty).Trim())).ToListAsync();

        var latestByIp = events
            .Where(e => !string.IsNullOrWhiteSpace(e.DeviceIP))
            .GroupBy(e => e.DeviceIP!.Trim())
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc ?? DateTime.MinValue).First()
            );

        var result = new List<CardPulseDto>();

        foreach (var d in deviceList)
        {
            var ip = (d.Ip ?? string.Empty).Trim();
            latestByIp.TryGetValue(ip, out var evt);

            DateTime stampBase = (evt?.UpdatedAtUtc
                                  ?? evt?.CreatedAtUtc
                                  ?? d.UpdatedAtUtc
                                  ?? d.CreatedAtUtc
                                  ?? DateTime.MinValue).ToUniversalTime();

            var cw = evt?.ColdWaterMeter;
            var hw = evt?.HotWaterMeter;
            var em = evt?.ElectricityMeter;

            var dg = evt?.Degree;
            var hm = evt?.Humidity;

            var laserUrl = Url.Content(IconPathFromLaser(evt));
            var soundUrl = Url.Content(IconPathFromSound(evt));
            var intercomUrl = Url.Content(IconPathFromIntercom(evt));
            var doorUrl = Url.Content(IconPathFromDoor(evt));

            result.Add(new CardPulseDto(
                Ip: ip,
                UpdatedAtTicks: stampBase.Ticks,
                IconUrl: Url.Content(IconPathFromEmergency(evt)),
                ShowOverlay: d.Status?.Equals("Disconnected", StringComparison.OrdinalIgnoreCase) == true,

                ColdIconUrl: cw != null ? Url.Content(IconPathForCold(cw)) : null,
                ColdConsumed: cw?.Consumed,

                HotIconUrl: hw != null ? Url.Content(IconPathForHot(hw)) : null,
                HotConsumed: hw?.Consumed,

                ElecIconUrl: em != null ? Url.Content(IconPathForElec(em)) : null,
                ElecConsumption: em?.Consumption,

                DegreeIconUrl: dg != null ? Url.Content(IconPathForDegree(dg)) : null,
                DegreeValue: dg?.Value,

                HumidityIconUrl: hm != null ? Url.Content(IconPathForHumidity(hm)) : null,
                HumidityValue: hm?.Value,

                LaserIconUrl: laserUrl,
                SoundIconUrl: soundUrl,
                IntercomIconUrl: intercomUrl,
                DoorIconUrl: doorUrl
            ));
        }

        return new JsonResult(result);
    }
}

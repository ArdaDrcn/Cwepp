using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using CimaxWeb2.Models;

namespace CimaxWeb2.Pages;

public class IndexModel : PageModel
{
    private readonly IMongoCollection<Device> _devices;
    private readonly IMongoCollection<PrisonEvent> _events;
    private readonly IMongoCollection<InterlockEvent> _interlocks;

    public List<DeviceCardVM> Cards { get; private set; } = new();

    public IndexModel(
        IMongoCollection<Device> devices,
        IMongoCollection<PrisonEvent> eventsCol,
        IMongoCollection<InterlockEvent> interlocksCol)
    {
        _devices = devices;
        _events = eventsCol;
        _interlocks = interlocksCol;
    }

    public async Task OnGet()
    {
        var deviceList = await _devices.Find(_ => true).Limit(200).ToListAsync();
        var ips = deviceList.Where(d => !string.IsNullOrWhiteSpace(d.Ip))
                            .Select(d => d.Ip!.Trim())
                            .Distinct()
                            .ToList();

        // PrisonEvent: IP bazlı en son
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

        // InterlockEvent: IP bazlı en son
        var interlocks = ips.Count == 0
            ? new List<InterlockEvent>()
            : await _interlocks.Find(e => ips.Contains((e.DeviceIP ?? string.Empty).Trim())).ToListAsync();

        var latestInterlockByIp = interlocks
            .Where(e => !string.IsNullOrWhiteSpace(e.DeviceIP))
            .GroupBy(e => e.DeviceIP!.Trim())
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.UpdatedAtUtc ?? x.CreatedAtUtc ?? DateTime.MinValue).First()
            );

        // Kartlar (her IP için 1 ana kart; Interlock kartlarını View ikinci foreach ile basıyoruz)
        var list = new List<DeviceCardVM>();
        foreach (var d in deviceList)
        {
            var ip = (d.Ip ?? string.Empty).Trim();
            latestByIp.TryGetValue(ip, out var evt);
            latestInterlockByIp.TryGetValue(ip, out var iEvt);

            list.Add(new DeviceCardVM
            {
                CardKey = $"{ip}|device",
                Type = CardType.Device,
                Device = d,
                LatestEvent = evt,
                LatestInterlockEvent = iEvt,
                Title = (d.Name ?? "(isimsiz cihaz)") + (string.IsNullOrWhiteSpace(d.Location) ? "" : $" - {d.Location}"),
                IconPath = IconPathFromEmergency(evt)
            });
        }

        Cards = list;
    }

    // --- İkon yolları (PATH) ---
    public static string IconPathFromEmergency(PrisonEvent? evt)
    {
        var code = (evt?.EmergencyCall ?? 0) == 1 ? 1 : 0;
        return code == 1 ? "~/img/GardiyanAktifGif.gif" : "~/img/AcilCagriPasif.png";
    }

    private static string InterlockDoorIconPath(bool active) =>
        active ? "~/img/GirisKapiAktif.png" : "~/img/GirisKapiPasif.png";

    // --- PULSE DTO ---
    public record CardPulseDto(
        string Ip,
        long UpdatedAtTicks,
        string IconUrl,
        bool ShowOverlay,

        // Prison yanları (grup görünümü için kullanıyorsan burada tut)
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
        string? DoorIconUrl,

        // Interlock
        string? InterlockDoor1IconUrl,
        string? InterlockDoor2IconUrl,
        string? InterlockDoor1Value,
        string? InterlockDoor2Value
    );

    public async Task<IActionResult> OnGetPulse()
    {
        var deviceList = await _devices.Find(_ => true).Limit(200).ToListAsync();
        var ips = deviceList.Where(d => !string.IsNullOrWhiteSpace(d.Ip))
                            .Select(d => d.Ip!.Trim())
                            .Distinct()
                            .ToList();

        // PrisonEvent latest
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

        // InterlockEvent latest
        var interlocks = ips.Count == 0
            ? new List<InterlockEvent>()
            : await _interlocks.Find(e => ips.Contains((e.DeviceIP ?? string.Empty).Trim())).ToListAsync();

        var latestInterlockByIp = interlocks
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
            latestInterlockByIp.TryGetValue(ip, out var iEvt);

            DateTime stampBase = new[]
            {
                (evt?.UpdatedAtUtc ?? evt?.CreatedAtUtc)?.ToUniversalTime(),
                (iEvt?.UpdatedAtUtc ?? iEvt?.CreatedAtUtc)?.ToUniversalTime(),
                (d.UpdatedAtUtc ?? d.CreatedAtUtc)?.ToUniversalTime()
            }.Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty(DateTime.MinValue).Max();

            var cw = evt?.ColdWaterMeter;
            var hw = evt?.HotWaterMeter;
            var em = evt?.ElectricityMeter;
            var dg = evt?.Degree;
            var hm = evt?.Humidity;

            result.Add(new CardPulseDto(
                Ip: ip,
                UpdatedAtTicks: stampBase.Ticks,
                IconUrl: Url.Content(IconPathFromEmergency(evt)),
                ShowOverlay: d.Status?.Equals("Disconnected", StringComparison.OrdinalIgnoreCase) == true,

                ColdIconUrl: cw != null ? Url.Content((cw.Status == "1" || cw.Status?.ToLower() == "true") ? "~/img/SuSayacSogukAktif.png" : "~/img/SuSayacSogukPasif.png") : null,
                ColdConsumed: cw?.Consumed,

                HotIconUrl: hw != null ? Url.Content((hw.Status == "1" || hw.Status?.ToLower() == "true") ? "~/img/SuSayacSicakAktif.png" : "~/img/SuSayacSicakPasif.png") : null,
                HotConsumed: hw?.Consumed,

                ElecIconUrl: em != null ? Url.Content((em.Status == "1" || em.Status?.ToLower() == "true") ? "~/img/ElektrikSayacAktif.png" : "~/img/ElektrikSayacPasif.png") : null,
                ElecConsumption: em?.Consumption,

                DegreeIconUrl: dg != null ? Url.Content((dg.Status == "1" || string.Equals(dg.Status, "true", StringComparison.OrdinalIgnoreCase)) ? "~/img/DereceAktif.png" : "~/img/DerecePasif.png") : null,
                DegreeValue: dg?.Value,

                HumidityIconUrl: hm != null ? Url.Content((hm.Status == "1" || string.Equals(hm.Status, "true", StringComparison.OrdinalIgnoreCase)) ? "~/img/NemAktif.png" : "~/img/NemPasif.png") : null,
                HumidityValue: hm?.Value,

                LaserIconUrl: Url.Content(((evt?.Laser ?? 0) == 1) ? "~/img/LazerAktif.png" : "~/img/LazerPasif.png"),
                SoundIconUrl: Url.Content((evt?.Sound ?? 0) switch
                {
                    0 => "~/img/HoparlorKopuk.png",
                    1 => "~/img/HoparlorPasif.png",
                    2 => "~/img/HoparlorAktif1.png",
                    3 => "~/img/HoparlorAktif2.png",
                    4 => "~/img/HoparlorAktif3.png",
                    5 => "~/img/HoparlorAktif4.png",
                    _ => "~/img/HoparlorKopuk.png"
                }),
                IntercomIconUrl: Url.Content(((evt?.Intercom ?? 0) == 1) ? "~/img/InterkomAktif.png" : "~/img/InterkomPasif.png"),
                DoorIconUrl: Url.Content(((evt?.Door ?? 0) == 1) ? "~/img/GirisKapiAktif.png" : "~/img/GirisKapiPasif.png"),

                InterlockDoor1IconUrl: iEvt != null ? Url.Content(InterlockDoorIconPath(iEvt.Door1Active)) : null,
                InterlockDoor2IconUrl: iEvt != null ? Url.Content(InterlockDoorIconPath(iEvt.Door2Active)) : null,
                InterlockDoor1Value: iEvt?.Door1Value,
                InterlockDoor2Value: iEvt?.Door2Value
            ));
        }

        return new JsonResult(result);
    }
}

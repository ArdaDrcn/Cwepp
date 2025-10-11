using MongoDB.Driver;
using CimaxWeb2.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Mongo ayarları
var mongo = builder.Configuration.GetSection("Mongo");
var connStr = mongo["ConnectionString"]!;
var dbName = mongo["Database"]!;
var devCol = mongo["DevicesCollection"]!;
var evtCol = mongo["EventsCollection"]!;
// İstersen appsettings’e bunu da eklersin:
var interlockCol = mongo["InterlockEventsCollection"] ?? "interlockevents";

// IMongoCollection<T> servisleri (senin mevcut kalıbın)
builder.Services.AddSingleton<IMongoCollection<Device>>(_ =>
{
    var client = new MongoClient(connStr);
    var db = client.GetDatabase(dbName);
    return db.GetCollection<Device>(devCol);
});

builder.Services.AddSingleton<IMongoCollection<PrisonEvent>>(_ =>
{
    var client = new MongoClient(connStr);
    var db = client.GetDatabase(dbName);
    return db.GetCollection<PrisonEvent>(evtCol);
});

// ✅ InterlockEvent de aynı kalıpta
builder.Services.AddSingleton<IMongoCollection<InterlockEvent>>(_ =>
{
    var client = new MongoClient(connStr);
    var db = client.GetDatabase(dbName);
    return db.GetCollection<InterlockEvent>(interlockCol);
});

var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

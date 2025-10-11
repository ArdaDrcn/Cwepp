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
var interlockCol = mongo["InterlockEventsCollection"] ?? "interlockevents";

// Tekil client & database (temiz ve performanslı DI)
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connStr));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

// Koleksiyonlar
builder.Services.AddSingleton<IMongoCollection<Device>>(sp =>
    sp.GetRequiredService<IMongoDatabase>().GetCollection<Device>(devCol));

builder.Services.AddSingleton<IMongoCollection<PrisonEvent>>(sp =>
    sp.GetRequiredService<IMongoDatabase>().GetCollection<PrisonEvent>(evtCol));

builder.Services.AddSingleton<IMongoCollection<InterlockEvent>>(sp =>
    sp.GetRequiredService<IMongoDatabase>().GetCollection<InterlockEvent>(interlockCol));

var app = builder.Build();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

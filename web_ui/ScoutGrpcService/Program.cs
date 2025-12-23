using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using ScoutGrpcService.Data;
using ScoutGrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Kestrel HTTP/2 ayarları
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/2 için port 5001
    options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
});

builder.Services.AddDbContext<ScoutDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ML servisi için HttpClient
builder.Services.AddHttpClient();

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<PlayerGrpcService>();
app.MapGet("/", () => "gRPC Player Service çalışıyor! Port: 5001");

app.Run();
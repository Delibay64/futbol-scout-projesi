using Microsoft.EntityFrameworkCore;
using ScoutGrpcService.Data;
using ScoutGrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ScoutDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddGrpc();
builder.Services.AddControllers(); // YENİ

var app = builder.Build();

app.MapGrpcService<PlayerGrpcService>();
app.MapControllers(); // YENİ
app.MapGet("/", () => "gRPC Player Service çalışıyor! Port: 5001");

app.Run();
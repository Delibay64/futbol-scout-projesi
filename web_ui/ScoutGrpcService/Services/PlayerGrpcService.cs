using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ScoutGrpcService.Data;
using System.Text;
using System.Text.Json;

namespace ScoutGrpcService.Services
{
    public class PlayerGrpcService : PlayerService.PlayerServiceBase
    {
        private readonly ScoutDbContext _context;
        private readonly ILogger<PlayerGrpcService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string ML_SERVICE_URL = "http://localhost:5000";

        public PlayerGrpcService(
            ScoutDbContext context,
            ILogger<PlayerGrpcService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public override async Task<PlayerResponse> GetPlayer(PlayerRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC: Oyuncu istendi - ID: {request.PlayerId}");

            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == request.PlayerId);

            if (player == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Oyuncu bulunamadı"));
            }

            return new PlayerResponse
            {
                PlayerId = player.PlayerId,
                FullName = player.FullName ?? "Bilinmiyor",
                Position = player.Position ?? "Bilinmiyor",
                Age = player.Age ?? 0,
                MarketValue = (double)(player.CurrentMarketValue ?? 0),
                TeamName = player.Team?.TeamName ?? "Bilinmiyor"
            };
        }

        public override async Task<PredictionResponse> PredictValue(PredictionRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"gRPC: ML Tahmini istendi - Player ID: {request.PlayerId}");

            try
            {
                // Veritabanından oyuncu bilgilerini çek
                var player = await _context.Players
                    .Include(p => p.Team)
                    .FirstOrDefaultAsync(p => p.PlayerId == request.PlayerId);

                if (player == null)
                {
                    _logger.LogWarning($"Oyuncu bulunamadı: {request.PlayerId}");
                    return new PredictionResponse
                    {
                        PredictedValue = 0,
                        Status = "error",
                        Message = "Oyuncu bulunamadı"
                    };
                }

                // Stats'ı ayrı çek
                var stats = await _context.Playerstats
                    .FirstOrDefaultAsync(s => s.PlayerId == request.PlayerId);

                // Python ML servisine gönderilecek veri
                var mlData = new
                {
                    Oyuncu = player.FullName ?? "Unknown",
                    Takim = player.Team?.TeamName ?? "Unknown",
                    Lig = player.Team?.LeagueName ?? "Unknown",
                    Ana_Mevki = player.Position ?? "Merkez Ortasaha",
                    Yas = player.Age ?? 25,
                    Ayak = "Sag", // Default
                    Gol = stats?.Goals ?? 0,
                    Asist = stats?.Assists ?? 0,
                    Sari_Kart = 0, // YellowCards alanı yok
                    Kirmizi_Kart = 0, // RedCards alanı yok
                    Maclar = stats?.MatchesPlayed ?? 0,
                    Dakika = 0 // MinutesPlayed alanı yok, Maclar * 90 kullanılabilir
                };

                // Python ML servisine HTTP POST isteği
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var jsonContent = JsonSerializer.Serialize(mlData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation($"ML Servisi çağrılıyor: {ML_SERVICE_URL}/predict");

                var response = await client.PostAsync($"{ML_SERVICE_URL}/predict", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

                    if (result.GetProperty("status").GetString() == "success")
                    {
                        var predictedValue = result.GetProperty("tahmini_deger").GetDouble();

                        _logger.LogInformation($"✅ ML Tahmini: {predictedValue:N0} EUR (Oyuncu: {player.FullName})");

                        return new PredictionResponse
                        {
                            PredictedValue = predictedValue,
                            Status = "success",
                            Message = $"ML Model Tahmini - {player.FullName}"
                        };
                    }
                }

                // ML servisi çalışmıyorsa basit formül kullan
                _logger.LogWarning("ML servisi cevap vermedi, basit formül kullanılıyor");

                double baseValue = 1000000;
                double goalBonus = (stats?.Goals ?? 0) * 50000;
                double assistBonus = (stats?.Assists ?? 0) * 30000;
                double ageMultiplier = (player.Age ?? 25) < 23 ? 1.5 : ((player.Age ?? 25) > 30 ? 0.7 : 1.0);
                double fallbackValue = (baseValue + goalBonus + assistBonus) * ageMultiplier;

                return new PredictionResponse
                {
                    PredictedValue = fallbackValue,
                    Status = "success",
                    Message = "Basit formül (ML servisi çalışmıyor)"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ML tahmin hatası");
                return new PredictionResponse
                {
                    PredictedValue = 0,
                    Status = "error",
                    Message = ex.Message
                };
            }
        }
    }
}
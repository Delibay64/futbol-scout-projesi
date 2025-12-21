using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ScoutGrpcService.Data;

namespace ScoutGrpcService.Services
{
    public class PlayerGrpcService : PlayerService.PlayerServiceBase
    {
        private readonly ScoutDbContext _context;
        private readonly ILogger<PlayerGrpcService> _logger;

        public PlayerGrpcService(ScoutDbContext context, ILogger<PlayerGrpcService> logger)
        {
            _context = context;
            _logger = logger;
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
            _logger.LogInformation($"gRPC: Tahmin istendi - Player ID: {request.PlayerId}");

            try
            {
                double baseValue = 1000000;
                double goalBonus = request.Goals * 50000;
                double assistBonus = request.Assists * 30000;
                double ageMultiplier = request.Age < 23 ? 1.5 : (request.Age > 30 ? 0.7 : 1.0);

                double predictedValue = (baseValue + goalBonus + assistBonus) * ageMultiplier;

                return await Task.FromResult(new PredictionResponse
                {
                    PredictedValue = predictedValue,
                    Status = "success",
                    Message = "Tahmin başarılı (gRPC)"
                });
            }
            catch (Exception ex)
            {
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
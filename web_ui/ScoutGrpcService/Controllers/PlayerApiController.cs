using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutGrpcService.Data;

namespace ScoutGrpcService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly ScoutDbContext _context;

        public PlayerController(ScoutDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _context.Players
                .Include(p => p.Team)
                .FirstOrDefaultAsync(p => p.PlayerId == id);

            if (player == null)
                return NotFound(new { message = "Oyuncu bulunamadı" });

            return Ok(new
            {
                playerId = player.PlayerId,
                fullName = player.FullName,
                position = player.Position,
                age = player.Age,
                marketValue = player.CurrentMarketValue,
                teamName = player.Team?.TeamName
            });
        }
    }
}
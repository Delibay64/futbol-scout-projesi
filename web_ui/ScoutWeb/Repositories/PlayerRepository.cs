using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;

namespace ScoutWeb.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ScoutDbContext _context;

        public PlayerRepository(ScoutDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync(string? searchString = null)
        {
            var query = _context.Players.Include(p => p.Team).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.FullName != null && 
                                        p.FullName.ToLower().Contains(searchString.ToLower()));
            }

            return await query.OrderByDescending(p => p.CurrentMarketValue)
                             .Take(100)
                             .ToListAsync();
        }

        public async Task<Player?> GetPlayerByIdAsync(int id)
        {
            return await _context.Players
                .Include(p => p.Team)
                .Include(p => p.Playerstats)
                .FirstOrDefaultAsync(p => p.PlayerId == id);
        }

        public async Task AddPlayerAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePlayerAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }
    }
}
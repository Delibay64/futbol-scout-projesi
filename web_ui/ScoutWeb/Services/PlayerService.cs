using Microsoft.EntityFrameworkCore;
using ScoutWeb.Models;
using ScoutWeb.Repositories;

namespace ScoutWeb.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ScoutDbContext _context;

        public PlayerService(IPlayerRepository playerRepository, ScoutDbContext context)
        {
            _playerRepository = playerRepository;
            _context = context;
        }

        public async Task<IEnumerable<Player>> GetPlayersAsync(string? searchString = null)
        {
            return await _playerRepository.GetAllPlayersAsync(searchString);
        }

        public async Task<Player?> GetPlayerDetailsAsync(int id)
        {
            return await _playerRepository.GetPlayerByIdAsync(id);
        }

        public async Task CreatePlayerAsync(Player player, string? newTeamName = null)
        {
            // İş mantığı: Takım yoksa oluştur
            if ((player.TeamId == null || player.TeamId == 0) && !string.IsNullOrEmpty(newTeamName))
            {
                var existingTeam = await _context.Teams
                    .FirstOrDefaultAsync(t => t.TeamName.ToLower() == newTeamName.ToLower());

                if (existingTeam != null)
                {
                    player.TeamId = existingTeam.TeamId;
                }
                else
                {
                    var newTeam = new Team
                    {
                        TeamName = newTeamName,
                        LeagueName = "Diğer"
                    };
                    _context.Teams.Add(newTeam);
                    await _context.SaveChangesAsync();
                    player.TeamId = newTeam.TeamId;
                }
            }

            await _playerRepository.AddPlayerAsync(player);
        }
    }
}
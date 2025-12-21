using ScoutWeb.Models;

namespace ScoutWeb.Services
{
    public interface IPlayerService
    {
        Task<IEnumerable<Player>> GetPlayersAsync(string? searchString = null);
        Task<Player?> GetPlayerDetailsAsync(int id);
        Task CreatePlayerAsync(Player player, string? newTeamName = null);
    }
}
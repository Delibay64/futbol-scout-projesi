using ScoutWeb.Models;

namespace ScoutWeb.Repositories
{
    public interface IPlayerRepository
    {
        Task<IEnumerable<Player>> GetAllPlayersAsync(string? searchString = null);
        Task<Player?> GetPlayerByIdAsync(int id);
        Task AddPlayerAsync(Player player);
        Task UpdatePlayerAsync(Player player);
        Task DeletePlayerAsync(int id);
    }
}
using ScoutWeb.Models;

namespace ScoutWeb.BusinessLogic
{
    public interface IValidationService
    {
        bool ValidatePlayer(Player player, out string errorMessage);
    }
}
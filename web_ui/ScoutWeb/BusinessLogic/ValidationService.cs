using ScoutWeb.Models;

namespace ScoutWeb.BusinessLogic
{
    public class ValidationService : IValidationService
    {
        public bool ValidatePlayer(Player player, out string errorMessage)
        {
            errorMessage = string.Empty;

            // İş kuralı 1: Yaş kontrolü
            if (player.Age.HasValue && (player.Age <= 0 || player.Age > 100))
            {
                errorMessage = "Oyuncu yaşı 0-100 arasında olmalıdır.";
                return false;
            }

            // İş kuralı 2: Piyasa değeri kontrolü
            if (player.CurrentMarketValue.HasValue && player.CurrentMarketValue < 0)
            {
                errorMessage = "Piyasa değeri negatif olamaz.";
                return false;
            }

            // İş kuralı 3: İsim kontrolü
            if (string.IsNullOrWhiteSpace(player.FullName))
            {
                errorMessage = "Oyuncu adı boş olamaz.";
                return false;
            }

            return true;
        }
    }
}
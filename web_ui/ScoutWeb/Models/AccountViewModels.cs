using System.ComponentModel.DataAnnotations;

namespace ScoutWeb.Models
{
    // Giriş Yaparken Kullanacağımız Model
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = null!; // "= null!" ekledik, hata sustu.

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }

    // Kayıt Olurken Kullanacağımız Model
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
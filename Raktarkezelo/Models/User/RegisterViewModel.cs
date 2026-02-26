using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.User
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email megadása kötelező")]
        [EmailAddress(ErrorMessage = "Helytelen email formátum")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Felhasználónév megadása kötelező")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jelszó megadása kötelező")]
        [MinLength(8, ErrorMessage = "A jelszónak legalább 8 karakter hosszúnak kell lennie")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megerősítése kötelező")]
        [Compare("Password", ErrorMessage = "A két jelszó nem egyezik")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
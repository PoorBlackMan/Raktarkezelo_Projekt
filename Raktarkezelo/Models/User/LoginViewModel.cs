using System.ComponentModel.DataAnnotations;
namespace Raktarkezelo.Models.User
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email megadása kötelező")]
        [EmailAddress(ErrorMessage = "Helytelen email formátum")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jelszó megadása kötelező")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
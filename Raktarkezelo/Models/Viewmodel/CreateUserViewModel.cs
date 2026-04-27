using Raktarkezelo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Viewmodel
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Az email megadása kötelező.")]
        [EmailAddress(ErrorMessage = "Helytelen email formátum.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A felhasználónév megadása kötelező.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "A felhasználónév 3-50 karakter hosszú lehet.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megadása kötelező.")]
        [MinLength(8, ErrorMessage = "A jelszónak legalább 8 karakter hosszúnak kell lennie.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megerősítése kötelező.")]
        [Compare("Password", ErrorMessage = "A két jelszó nem egyezik.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A szerepkör megadása kötelező.")]
        public UserRole Role { get; set; } = UserRole.User;
    }
}

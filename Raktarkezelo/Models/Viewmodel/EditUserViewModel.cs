using Raktarkezelo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Viewmodel
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Az email megadása kötelező.")]
        [EmailAddress(ErrorMessage = "Helytelen email formátum.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A felhasználónév megadása kötelező.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "A felhasználónév 3-50 karakter hosszú lehet.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A szerepkör megadása kötelező.")]
        public UserRole Role { get; set; }

        public bool IsActive { get; set; }
    }
}

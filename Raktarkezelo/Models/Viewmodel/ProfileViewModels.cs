using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Viewmodel
{
    public class ChangeUsernameViewModel
    {
        [Required(ErrorMessage = "Az új felhasználónév megadása kötelező.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "A felhasználónév 3-50 karakter hosszú lehet.")]
        [Display(Name = "Új felhasználónév")]
        public string NewUsername { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "A jelenlegi jelszó megadása kötelező.")]
        [DataType(DataType.Password)]
        [Display(Name = "Jelenlegi jelszó")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Az új jelszó megadása kötelező.")]
        [MinLength(8, ErrorMessage = "A jelszónak legalább 8 karakter hosszúnak kell lennie.")]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megerősítése kötelező.")]
        [Compare("NewPassword", ErrorMessage = "A két jelszó nem egyezik.")]
        [DataType(DataType.Password)]
        [Display(Name = "Új jelszó megerősítése")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

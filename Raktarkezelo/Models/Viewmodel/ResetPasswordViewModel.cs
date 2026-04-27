using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Viewmodel
{
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Az új jelszó megadása kötelező.")]
        [MinLength(8, ErrorMessage = "A jelszónak legalább 8 karakter hosszúnak kell lennie.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A jelszó megerősítése kötelező.")]
        [Compare("NewPassword", ErrorMessage = "A két jelszó nem egyezik.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

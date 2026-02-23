using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Raktarkezelo.Models.User
{
    public class Userinfo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nem adtál meg emailt, vagy helytelen emailt adtál meg")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nincs megadva felhasználónév")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nem adtál meg jelszót!")]
        [MinLength(8, ErrorMessage = "A jelszónak legalább 8 karakter hosszúnak kell lennie.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        [Required(ErrorMessage = "A jelszó megerősítése kötelező")]
        [Compare("Password", ErrorMessage = "A két jelszó nem egyezik")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
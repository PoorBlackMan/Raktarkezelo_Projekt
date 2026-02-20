using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.User_Info
{
    public class Register
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Nem adtál meg emailt, vagy helytelen emailt adtál meg")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nem adtál meg jelszót, vagy a jelszavad nem elég erős! (Legalább 8 karakter, kis és nagy betű használata!)")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nincs megadva felhasználónév")]
        public string Username { get; set; } = string.Empty;


    }
}

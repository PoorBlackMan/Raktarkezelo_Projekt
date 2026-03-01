using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Raktarkezelo.Models.Entities
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
        public string Passwordhash { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = "Felhasználó";


        [NotMapped]
        [Required(ErrorMessage = "A jelszó megerősítése kötelező")]
        [Compare("Password", ErrorMessage = "A két jelszó nem egyezik")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<StockTransactions> StockTransactions { get; set; } = new List<StockTransactions>();

    }
}
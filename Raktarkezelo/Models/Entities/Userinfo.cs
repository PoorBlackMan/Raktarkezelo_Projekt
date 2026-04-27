using Raktarkezelo.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Entities
{
    public class Userinfo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Passwordhash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public ICollection<StockTransactions> StockTransactions { get; set; } = new List<StockTransactions>();
    }
}
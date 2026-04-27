using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raktarkezelo.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // Who did it
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public Userinfo? User { get; set; }

        // What username at the time (kept even if user deleted)
        [Required]
        public string Username { get; set; } = string.Empty;

        // Action category: Login, Logout, ProductCreate, ProductEdit, ProductDelete,
        // StockIn, StockOut, StockAdjust, UserCreate, UserEdit, UserDelete,
        // UserToggle, PasswordReset, ProfileEdit
        [Required]
        public string Action { get; set; } = string.Empty;

        // Human-readable description
        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

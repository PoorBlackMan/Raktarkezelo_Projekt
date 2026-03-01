using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Entities
{
    public class StockTransactions
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Products Product { get; set; } = null!;

        [Required]
        public int UserId { get; set; }
        public Userinfo User { get; set; } = null!;

        // IN / OUT / ADJUST
        [Required]
        public string Type { get; set; } = "IN";

        [Required]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
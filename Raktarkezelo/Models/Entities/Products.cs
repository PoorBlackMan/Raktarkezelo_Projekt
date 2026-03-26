using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Entities
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "A név megadása kötelező.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "A kategória megadása kötelező.")]
        [StringLength(50)]
        public string Category { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A készlet nem lehet negatív.")]
        public int Stock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A minimum készlet nem lehet negatív.")]
        public int MinStock { get; set; }

        [StringLength(250)]
        public string? Note { get; set; }

        public ICollection<StockTransactions> StockTransactions { get; set; } = new List<StockTransactions>();
    }
}
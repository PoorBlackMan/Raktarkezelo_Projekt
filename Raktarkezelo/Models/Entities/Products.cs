using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Entities
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; } 
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Range (0,200)]
        public int Stock { get; set; }
        public string Note { get; set; } = string.Empty;
        public ICollection<StockTransactions> StockTransactions { get; set; } = new List<StockTransactions>();


    }
}

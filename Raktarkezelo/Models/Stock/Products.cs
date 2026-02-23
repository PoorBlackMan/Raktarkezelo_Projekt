using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Stock
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

    }
}

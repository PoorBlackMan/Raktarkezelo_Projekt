using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Viewmodel
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int Stock { get; set; }

        [Required]
        public int MinStock { get; set; }

        public string Note { get; set; } = string.Empty;
    }
}
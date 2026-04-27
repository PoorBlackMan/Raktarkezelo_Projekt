using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Viewmodel
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "A megnevezés megadása kötelező.")]
        [Display(Name = "Megnevezés")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A kategória megadása kötelező.")]
        [Display(Name = "Kategória")]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A készlet nem lehet negatív.")]
        [Display(Name = "Készlet")]
        public int Stock { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A minimum készlet nem lehet negatív.")]
        [Display(Name = "Minimum készlet")]
        public int MinStock { get; set; }

        [Display(Name = "Megjegyzés")]
        public string? Note { get; set; }
    }
}

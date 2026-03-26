using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raktarkezelo.Models.Viewmodel
{
    public class AdjustmentViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Új készlet megadása kötelező.")]
        public int NewQuantity { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public List<SelectListItem> Products { get; set; } = new();
    }
}
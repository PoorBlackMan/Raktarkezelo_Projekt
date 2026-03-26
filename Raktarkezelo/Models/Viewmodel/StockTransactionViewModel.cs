using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Raktarkezelo.Models.Viewmodel
{
    public class StockTransactionViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "A mennyiség megadása kötelező.")]
        [Range(1, int.MaxValue, ErrorMessage = "A mennyiségnek legalább 1-nek kell lennie.")]
        public int Quantity { get; set; }

        public string Note { get; set; } = string.Empty;

        public List<SelectListItem> Products { get; set; } = new();
    }
}
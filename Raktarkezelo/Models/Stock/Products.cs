using System.ComponentModel.DataAnnotations;

namespace Raktarkezelo.Models.Stock
{
    public class Products
    {
        
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
       
        [Range (0,200)]
        public int Stock { get; set; }
        public string Note { get; set; } = string.Empty;

    }
}

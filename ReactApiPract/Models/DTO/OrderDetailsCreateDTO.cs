using ReactApiPract.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReactApiPract.Models.DTO
{
    public class OrderDetailsCreateDTO
    {
        
      
        [Required]
        public int MenuItemId { get; set; }
       
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public double Price { get; set; }
    }
}

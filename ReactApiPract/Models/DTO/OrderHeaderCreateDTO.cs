using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReactApiPract.Models.DTO
{
    public class OrderHeaderCreateDTO
    {
        public string PickupName { get; set; }
        [Required]
        public string PickupPhoneNumber { get; set; }
        [Required]
        public string PickupEmail { get; set; }
        public string ApplicationUserId { get; set; }
      
        public double OrderTotal { get; set; }
       
        public string StripePaymentIntentID { get; set; }
        public string Status { get; set; }
        public int TotalItems { get; set; }

        public IEnumerable<OrderDetailsCreateDTO> OrderDetailsDTO { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;

namespace ReactApiPract.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}

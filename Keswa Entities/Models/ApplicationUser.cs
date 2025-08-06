using Microsoft.AspNetCore.Identity;
namespace Keswa_Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Image { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }
    }
}

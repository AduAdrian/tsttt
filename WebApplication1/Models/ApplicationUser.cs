using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        // UserName este deja inclus �n IdentityUser
        // Email este deja inclus �n IdentityUser
        // PhoneNumber este deja inclus �n IdentityUser, dar s? �l fac obligatoriu
        public override string? PhoneNumber { get; set; }
        
        // Putem ad?uga ?i alte propriet??i dac? e necesar
        public string? DisplayName { get; set; }
    }
}
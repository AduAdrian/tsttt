using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        // UserName este deja inclus în IdentityUser
        // Email este deja inclus în IdentityUser
        // PhoneNumber este deja inclus în IdentityUser, dar s? îl fac obligatoriu
        public override string? PhoneNumber { get; set; }
        
        // Putem ad?uga ?i alte propriet??i dac? e necesar
        public string? DisplayName { get; set; }
    }
}
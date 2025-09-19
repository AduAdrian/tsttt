using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numarul de inmatriculare este obligatoriu")]
        [Display(Name = "Numar Inmatriculare")]
        [StringLength(15, ErrorMessage = "Numarul de inmatriculare nu poate depasi 15 caractere")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipul de valabilitate este obligatoriu")]
        [Display(Name = "Tip Valabilitate")]
        public ValidityType ValidityType { get; set; } = ValidityType.Manual;

        [Required(ErrorMessage = "Data de expirare ITP este obligatorie")]
        [Display(Name = "Data Expirare ITP")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Numar Telefon")]
        [StringLength(15, ErrorMessage = "Numarul de telefon nu poate depasi 15 caractere")]
        [Phone(ErrorMessage = "Formatul numarului de telefon nu este valid")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Data Crearii")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Activ")]
        public bool IsActive { get; set; } = true;

        // Optional: user who created the client
        public string? CreatedByUserId { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }

        // Navigation property for SMS notifications
        public virtual ICollection<SmsNotification> SmsNotifications { get; set; } = new List<SmsNotification>();
    }

    public enum ValidityType
    {
        [Display(Name = "Manual")]
        Manual = 0,
        
        [Display(Name = "6 Luni")]
        SixMonths = 6,
        
        [Display(Name = "1 An")]
        OneYear = 12,
        
        [Display(Name = "2 Ani")]
        TwoYears = 24
    }
}
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(200, ErrorMessage = "Titlul nu poate dep??i 200 de caractere")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Descrierea nu poate dep??i 1000 de caractere")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Data ?i ora sunt obligatorii")]
        public DateTime AppointmentDate { get; set; }
        
        [Required(ErrorMessage = "Durata este obligatorie")]
        [Range(15, 480, ErrorMessage = "Durata trebuie s? fie între 15 minute ?i 8 ore")]
        public int DurationMinutes { get; set; } = 60;
        
        [Required(ErrorMessage = "Numele clientului este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele nu poate dep??i 100 de caractere")]
        public string ClientName { get; set; } = string.Empty;
        
        [StringLength(15, ErrorMessage = "Num?rul de telefon nu poate dep??i 15 caractere")]
        public string? ClientPhone { get; set; }
        
        [EmailAddress(ErrorMessage = "Adresa de email nu este valid?")]
        [StringLength(100, ErrorMessage = "Email-ul nu poate dep??i 100 de caractere")]
        public string? ClientEmail { get; set; }
        
        [StringLength(200, ErrorMessage = "Loca?ia nu poate dep??i 200 de caractere")]
        public string? Location { get; set; }
        
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        
        [StringLength(500, ErrorMessage = "Notele nu pot dep??i 500 de caractere")]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public string? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }
        
        // Calculated properties
        public DateTime EndTime => AppointmentDate.AddMinutes(DurationMinutes);
        
        public bool IsToday => AppointmentDate.Date == DateTime.Today;
        
        public bool IsUpcoming => AppointmentDate > DateTime.Now;
        
        public bool IsPast => AppointmentDate.AddMinutes(DurationMinutes) < DateTime.Now;
    }
    
    public enum AppointmentStatus
    {
        [Display(Name = "Programat")]
        Scheduled = 0,
        
        [Display(Name = "Confirmat")]
        Confirmed = 1,
        
        [Display(Name = "În desf??urare")]
        InProgress = 2,
        
        [Display(Name = "Finalizat")]
        Completed = 3,
        
        [Display(Name = "Anulat")]
        Cancelled = 4,
        
        [Display(Name = "Reprogramat")]
        Rescheduled = 5
    }
}
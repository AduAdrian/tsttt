using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AppointmentListViewModel
    {
        public List<Appointment> Appointments { get; set; } = new();
        public DateTime? FilterDate { get; set; }
        public AppointmentStatus? FilterStatus { get; set; }
        public string? FilterClient { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
    }
    
    public class CreateAppointmentViewModel
    {
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(200, ErrorMessage = "Titlul nu poate dep??i 200 de caractere")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Descrierea nu poate dep??i 1000 de caractere")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Data program?rii este obligatorie")]
        [Display(Name = "Data program?rii")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddHours(9);
        
        [Required(ErrorMessage = "Durata este obligatorie")]
        [Range(15, 480, ErrorMessage = "Durata trebuie s? fie între 15 minute ?i 8 ore")]
        [Display(Name = "Durata (minute)")]
        public int DurationMinutes { get; set; } = 60;
        
        [Required(ErrorMessage = "Numele clientului este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele nu poate dep??i 100 de caractere")]
        [Display(Name = "Nume client")]
        public string ClientName { get; set; } = string.Empty;
        
        [StringLength(15, ErrorMessage = "Num?rul de telefon nu poate dep??i 15 caractere")]
        [Display(Name = "Telefon client")]
        public string? ClientPhone { get; set; }
        
        [EmailAddress(ErrorMessage = "Adresa de email nu este valid?")]
        [StringLength(100, ErrorMessage = "Email-ul nu poate dep??i 100 de caractere")]
        [Display(Name = "Email client")]
        public string? ClientEmail { get; set; }
        
        [StringLength(200, ErrorMessage = "Loca?ia nu poate dep??i 200 de caractere")]
        [Display(Name = "Loca?ia")]
        public string? Location { get; set; }
        
        [Display(Name = "Status")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        
        [StringLength(500, ErrorMessage = "Notele nu pot dep??i 500 de caractere")]
        [Display(Name = "Note")]
        public string? Notes { get; set; }
    }
    
    public class EditAppointmentViewModel : CreateAppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class AppointmentDetailsViewModel
    {
        public Appointment Appointment { get; set; } = new();
        public List<AppointmentHistoryEntry> History { get; set; } = new();
    }
    
    public class AppointmentHistoryEntry
    {
        public DateTime Date { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
    
    public class AppointmentCalendarViewModel
    {
        public DateTime CurrentMonth { get; set; } = DateTime.Today;
        public List<Appointment> Appointments { get; set; } = new();
        public List<CalendarDay> Days { get; set; } = new();
    }
    
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public List<Appointment> Appointments { get; set; } = new();
        public int AppointmentCount => Appointments.Count;
    }
}
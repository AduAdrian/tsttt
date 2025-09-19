using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class SmsNotification
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? SentAt { get; set; }
        public DateTime? ScheduledFor { get; set; }

        // Pentru tracking duplicate
        [Required]
        public NotificationType Type { get; set; }

        // Câmp îmbun?t??it pentru zilele de notificare
        [Required]
        public int NotificationDays { get; set; } // 30, 7, 1, 0

        [Required]
        public DateTime ExpiryDateSnapshot { get; set; } // Snapshot-ul datei de expirare pentru care s-a trimis notificarea

        public int RetryCount { get; set; } = 0;
        public int MaxRetryCount { get; set; } = 3;

        public string? CreatedByUserId { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }

        // Helper properties
        public bool CanRetry => RetryCount < MaxRetryCount && Status == NotificationStatus.Failed;
        public bool IsWithinBusinessHours => DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 21;
        
        // Proprietate pentru culoarea badge-ului bazat? pe zile
        public string GetColorByDays()
        {
            return NotificationDays switch
            {
                30 => "primary", // Albastru
                7 => "warning",  // Galben
                1 => "danger",   // Ro?u
                0 => "danger",   // Ro?u (expirat ast?zi)
                _ => "secondary" // Gri
            };
        }
    }

    public enum NotificationStatus
    {
        [Display(Name = "În a?teptare")]
        Pending = 0,

        [Display(Name = "Trimis cu succes")]
        Sent = 1,

        [Display(Name = "E?uat")]
        Failed = 2,

        [Display(Name = "Anulat")]
        Cancelled = 3,

        [Display(Name = "Programat")]
        Scheduled = 4
    }

    public enum NotificationType
    {
        [Display(Name = "ITP Expiring 30 Days")]
        ItpExpiring30Days = 1,

        [Display(Name = "ITP Expiring 7 Days")]
        ItpExpiring7Days = 2,

        [Display(Name = "ITP Expiring 1 Day")]
        ItpExpiring1Day = 3,

        [Display(Name = "ITP Expired")]
        ItpExpired = 4,

        [Display(Name = "Manual")]
        Manual = 5,

        // Pentru compatibilitate cu codul existent
        [Display(Name = "ITP Expiring")]
        ItpExpiring = 6,

        [Display(Name = "Reminder")]
        Reminder = 7
    }
}
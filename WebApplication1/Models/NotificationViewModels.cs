using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class NotificationListViewModel
    {
        public int Id { get; set; }
        public string ClientRegistrationNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; }
        public NotificationType Type { get; set; }
        public int NotificationDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int DaysUntilExpiry { get; set; }

        public string StatusBadgeClass =>
            Status switch
            {
                NotificationStatus.Sent => "badge bg-success",
                NotificationStatus.Failed => "badge bg-danger", 
                NotificationStatus.Pending => "badge bg-warning",
                NotificationStatus.Scheduled => "badge bg-info",
                NotificationStatus.Cancelled => "badge bg-secondary",
                _ => "badge bg-secondary"
            };

        public string TypeBadgeClass =>
            NotificationDays switch
            {
                30 => "badge bg-primary",   // Albastru
                7 => "badge bg-warning",    // Galben
                1 => "badge bg-danger",     // Ro?u
                0 => "badge bg-danger",     // Ro?u (expirat)
                _ => "badge bg-secondary"   // Gri
            };

        public string GetDaysText() =>
            NotificationDays switch
            {
                30 => "30 zile",
                7 => "7 zile", 
                1 => "1 zi",
                0 => "Expirat",
                _ => $"{NotificationDays} zile"
            };

        public string GetRowColorClass() =>
            NotificationDays switch
            {
                30 => "table-primary",   // Albastru deschis
                7 => "table-warning",    // Galben deschis
                1 => "table-danger",     // Ro?u deschis
                0 => "table-danger",     // Ro?u deschis
                _ => ""
            };
    }

    public class NotificationDashboardViewModel
    {
        public int TotalNotifications { get; set; }
        public int SentToday { get; set; }
        public int PendingNotifications { get; set; }
        public int FailedNotifications { get; set; }
        public int ClientsEligibleForNotification { get; set; }
        public int ClientsWithoutPhone { get; set; }

        // Statistici pe zile
        public int Clients30Days { get; set; }
        public int Clients7Days { get; set; }
        public int Clients1Day { get; set; }
        
        public int Sent30Days { get; set; }
        public int Sent7Days { get; set; }
        public int Sent1Day { get; set; }
        
        public int NotSent30Days { get; set; }
        public int NotSent7Days { get; set; }
        public int NotSent1Day { get; set; }

        public List<NotificationListViewModel> RecentNotifications { get; set; } = new();
        public List<ClientListViewModel> UpcomingExpirations30Days { get; set; } = new();
        public List<ClientListViewModel> UpcomingExpirations7Days { get; set; } = new();
        public List<ClientListViewModel> UpcomingExpirations1Day { get; set; } = new();
        public List<ClientListViewModel> ClientsNeedingPhone { get; set; } = new();

        public double SuccessRate => TotalNotifications > 0 ? 
            Math.Round((double)(TotalNotifications - FailedNotifications) / TotalNotifications * 100, 1) : 0;

        public double SuccessRate30Days => Clients30Days > 0 ?
            Math.Round((double)Sent30Days / Clients30Days * 100, 1) : 0;

        public double SuccessRate7Days => Clients7Days > 0 ?
            Math.Round((double)Sent7Days / Clients7Days * 100, 1) : 0;

        public double SuccessRate1Day => Clients1Day > 0 ?
            Math.Round((double)Sent1Day / Clients1Day * 100, 1) : 0;
    }

    public class SendNotificationViewModel
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        [Display(Name = "Tip Notificare")]
        public NotificationType Type { get; set; } = NotificationType.ItpExpiring7Days;

        [Required]
        [Range(1, 30, ErrorMessage = "Zilele trebuie s? fie între 1 ?i 30")]
        [Display(Name = "Zile pân? la expirare")]
        public int DaysAhead { get; set; } = 7;

        [StringLength(1000)]
        [Display(Name = "Mesaj Custom (op?ional)")]
        public string? CustomMessage { get; set; }

        // Pentru afi?are
        public string ClientRegistrationNumber { get; set; } = string.Empty;
        public string? ClientPhoneNumber { get; set; }
        public DateTime ClientExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    public class NotificationHistoryViewModel
    {
        public List<NotificationListViewModel> Notifications { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        public string? FilterStatus { get; set; }
        public string? FilterType { get; set; }
        public string? FilterDays { get; set; }
        public DateTime? FilterDateFrom { get; set; }
        public DateTime? FilterDateTo { get; set; }
        public string? FilterClientRegistration { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class BulkNotificationViewModel
    {
        [Required]
        [Range(1, 30, ErrorMessage = "Num?rul de zile trebuie s? fie între 1 ?i 30")]
        [Display(Name = "Zile pân? la expirare")]
        public int DaysAhead { get; set; } = 7;

        [Required]
        [Display(Name = "Tip Notificare")]
        public NotificationType Type { get; set; } = NotificationType.ItpExpiring7Days;

        [StringLength(1000)]
        [Display(Name = "Mesaj Custom (op?ional)")]
        public string? CustomMessage { get; set; }

        [Display(Name = "Doar previzualizare (nu trimite)")]
        public bool PreviewOnly { get; set; } = true;

        // Pentru afi?are rezultate
        public List<ClientListViewModel> EligibleClients { get; set; } = new();
        public int TotalEligible { get; set; }
        public int SentSuccessfully { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
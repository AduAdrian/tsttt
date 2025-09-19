using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ClientCreateViewModel
    {
        [Required(ErrorMessage = "Numarul de inmatriculare este obligatoriu")]
        [Display(Name = "Nr. Inmatriculare")]
        [StringLength(15, ErrorMessage = "Numarul de inmatriculare nu poate depasi 15 caractere")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipul de valabilitate este obligatoriu")]
        [Display(Name = "Valabilitate")]
        public ValidityType ValidityType { get; set; } = ValidityType.Manual; // Default: Manual

        [Required(ErrorMessage = "Data de expirare ITP este obligatorie")]
        [Display(Name = "Data Expirare ITP")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; } // Will be set based on ValidityType

        [Display(Name = "Numar Telefon")]
        [StringLength(15, ErrorMessage = "Numarul de telefon nu poate depasi 15 caractere")]
        [Phone(ErrorMessage = "Formatul numarului de telefon nu este valid")]
        public string? PhoneNumber { get; set; }
    }

    public class ClientEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Numarul de inmatriculare este obligatoriu")]
        [Display(Name = "Nr. Inmatriculare")]
        [StringLength(15, ErrorMessage = "Numarul de inmatriculare nu poate depasi 15 caractere")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipul de valabilitate este obligatoriu")]
        [Display(Name = "Valabilitate")]
        public ValidityType ValidityType { get; set; }

        [Required(ErrorMessage = "Data de expirare ITP este obligatorie")]
        [Display(Name = "Data Expirare ITP")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Numar Telefon")]
        [StringLength(15, ErrorMessage = "Numarul de telefon nu poate depasi 15 caractere")]
        [Phone(ErrorMessage = "Formatul numarului de telefon nu este valid")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Activ")]
        public bool IsActive { get; set; }
    }

    public class ClientDetailsViewModel
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public ValidityType ValidityType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedByUserName { get; set; }

        // Calculated properties
        public int DaysUntilExpiry => (ExpiryDate - DateTime.Today).Days;
        public bool IsExpired => DateTime.Today > ExpiryDate;
        public bool IsExpiringSoon => DaysUntilExpiry <= 30 && DaysUntilExpiry >= 0;
        public bool IsEligibleForNotification => DaysUntilExpiry <= 7 && DaysUntilExpiry >= 0 && !string.IsNullOrEmpty(PhoneNumber);
    }

    public class ClientListViewModel
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public ValidityType ValidityType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }

        // Calculated properties
        public int DaysUntilExpiry => (ExpiryDate - DateTime.Today).Days;
        public bool IsExpired => DateTime.Today > ExpiryDate;
        public bool IsExpiringSoon => DaysUntilExpiry <= 30 && DaysUntilExpiry >= 0;
        public bool IsEligibleForNotification => DaysUntilExpiry <= 7 && DaysUntilExpiry >= 0 && !string.IsNullOrEmpty(PhoneNumber);
        
        public string StatusBadgeClass =>
            IsExpired ? "badge bg-danger" :
            IsExpiringSoon ? "badge bg-warning" :
            "badge bg-success";
            
        public string StatusText =>
            IsExpired ? "Expirat" :
            IsExpiringSoon ? "Expira curand" :
            "Valid";

        public string NotificationStatusText =>
            IsEligibleForNotification ? "Poate primi SMS" :
            string.IsNullOrEmpty(PhoneNumber) ? "Fara telefon" :
            "Nu necesita SMS";
    }

    public class PaginatedClientsViewModel
    {
        public List<ClientListViewModel> Clients { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

        // Statistics
        public int ExpiredCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public int ValidCount { get; set; }

        // Helper methods for pagination
        public List<int> GetPageNumbers()
        {
            var pages = new List<int>();
            var start = Math.Max(1, CurrentPage - 2);
            var end = Math.Min(TotalPages, CurrentPage + 2);

            for (int i = start; i <= end; i++)
            {
                pages.Add(i);
            }

            return pages;
        }
    }

    public class ClientSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "ExpiryDate";
        public string? SortDirection { get; set; } = "asc";
        public ClientStatusFilter StatusFilter { get; set; } = ClientStatusFilter.All;
    }

    public enum ClientStatusFilter
    {
        All,
        Valid,
        ExpiringSoon,
        Expired
    }
}
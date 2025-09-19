using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    /// <summary>
    /// Model pentru validarea avansat? a importului Excel
    /// </summary>
    public class ExcelImportValidationModel
    {
        [Required]
        [Display(Name = "Fisier Excel")]
        public IFormFile File { get; set; } = null!;

        [Range(1, 1000, ErrorMessage = "Numarul de randuri trebuie sa fie intre 1 si 1000")]
        public int MaxRows { get; set; } = 1000;

        public bool SkipDuplicates { get; set; } = true;
        public bool ValidatePhoneNumbers { get; set; } = true;
        public bool AllowFutureExpiryDates { get; set; } = true;
    }

    /// <summary>
    /// Model pentru rezultatul importului Excel cu detalii
    /// </summary>
    public class ExcelImportResult
    {
        public int TotalRowsProcessed { get; set; }
        public int SuccessfulImports { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public List<ImportError> ErrorDetails { get; set; } = new();
        public List<ImportWarning> WarningDetails { get; set; } = new();
        public List<string> SuccessMessages { get; set; } = new();
        public DateTime ImportDateTime { get; set; } = DateTime.Now;
        public string ImportedByUser { get; set; } = string.Empty;
        
        public bool HasErrors => Errors > 0;
        public bool HasWarnings => Warnings > 0;
        public bool IsSuccessful => SuccessfulImports > 0;
        public double SuccessRate => TotalRowsProcessed > 0 ? (double)SuccessfulImports / TotalRowsProcessed * 100 : 0;
    }

    /// <summary>
    /// Model pentru detaliile erorilor de import
    /// </summary>
    public class ImportError
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string InvalidValue { get; set; } = string.Empty;
        public ImportErrorType ErrorType { get; set; }
    }

    /// <summary>
    /// Model pentru avertismentele de import
    /// </summary>
    public class ImportWarning
    {
        public int RowNumber { get; set; }
        public string ColumnName { get; set; } = string.Empty;
        public string WarningMessage { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public ImportWarningType WarningType { get; set; }
    }

    /// <summary>
    /// Tipuri de erori pentru import
    /// </summary>
    public enum ImportErrorType
    {
        RequiredFieldMissing,
        InvalidFormat,
        DataTooLong,
        InvalidValue,
        DuplicateEntry,
        BusinessRuleViolation,
        DatabaseConstraint
    }

    /// <summary>
    /// Tipuri de avertismente pentru import
    /// </summary>
    public enum ImportWarningType
    {
        DataNormalized,
        OptionalFieldIgnored,
        DefaultValueUsed,
        PotentialDuplicate,
        DataTruncated,
        FormatCorrected
    }

    /// <summary>
    /// Configura?ie pentru validarea importului Excel
    /// </summary>
    public class ExcelImportConfiguration
    {
        public int MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB
        public int MaxRows { get; set; } = 1000;
        public string[] AllowedFileExtensions { get; set; } = { ".xlsx" };
        public bool ValidateHeaders { get; set; } = true;
        public bool AllowPartialImport { get; set; } = true;
        public double MaxErrorRate { get; set; } = 0.3; // 30%
        public bool NormalizeData { get; set; } = true;
        public bool SkipEmptyRows { get; set; } = true;
        
        public Dictionary<string, ColumnValidationRule> ColumnRules { get; set; } = new()
        {
            ["Nr. Inmatriculare"] = new ColumnValidationRule
            {
                IsRequired = true,
                MaxLength = 15,
                ValidationPattern = @"^[A-Z]{1,3}\d{2,3}[A-Z]{3}$",
                NormalizationFunction = "ToUpper"
            },
            ["Numar Telefon"] = new ColumnValidationRule
            {
                IsRequired = false,
                MaxLength = 15,
                ValidationPattern = @"^07\d{8}$",
                NormalizationFunction = "NormalizePhoneNumber"
            },
            ["Tip Valabilitate"] = new ColumnValidationRule
            {
                IsRequired = true,
                AllowedValues = new[] { "Manual", "6 Luni", "1 An", "2 Ani" },
                IsCaseInsensitive = true
            },
            ["Data Expirare ITP"] = new ColumnValidationRule
            {
                IsRequired = true,
                DataType = "DateTime",
                CustomValidator = "ValidateExpiryDate"
            }
        };
    }

    /// <summary>
    /// Reguli de validare pentru coloane
    /// </summary>
    public class ColumnValidationRule
    {
        public bool IsRequired { get; set; }
        public int MaxLength { get; set; } = int.MaxValue;
        public int MinLength { get; set; } = 0;
        public string ValidationPattern { get; set; } = string.Empty;
        public string[] AllowedValues { get; set; } = Array.Empty<string>();
        public bool IsCaseInsensitive { get; set; } = false;
        public string DataType { get; set; } = "String";
        public string NormalizationFunction { get; set; } = string.Empty;
        public string CustomValidator { get; set; } = string.Empty;
        public object DefaultValue { get; set; } = null!;
    }

    /// <summary>
    /// Statistici pentru monitorizarea importurilor
    /// </summary>
    public class ImportStatistics
    {
        public DateTime Date { get; set; }
        public int TotalImports { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public int TotalRecordsImported { get; set; }
        public double AverageProcessingTime { get; set; }
        public List<string> MostCommonErrors { get; set; } = new();
        public string ImportedByUser { get; set; } = string.Empty;
    }
}
namespace WebApplication1.Services
{
    public interface ISmsService
    {
        /// <summary>
        /// Trimite SMS cu cod pentru resetarea parolei
        /// </summary>
        /// <param name="phoneNumber">Num?rul de telefon în format românesc</param>
        /// <param name="resetCode">Codul de resetare (6 cifre)</param>
        /// <returns>Task care se completeaz? când SMS-ul este trimis</returns>
        Task SendPasswordResetSmsAsync(string phoneNumber, string resetCode);
        
        /// <summary>
        /// Trimite SMS cu cod de verificare
        /// </summary>
        /// <param name="phoneNumber">Num?rul de telefon în format românesc</param>
        /// <param name="code">Codul de verificare (6 cifre)</param>
        /// <returns>Task care se completeaz? când SMS-ul este trimis</returns>
        Task SendVerificationCodeAsync(string phoneNumber, string code);
        
        /// <summary>
        /// Trimite SMS generic
        /// </summary>
        /// <param name="phoneNumber">Num?rul de telefon în format românesc</param>
        /// <param name="message">Mesajul de trimis</param>
        /// <returns>True dac? SMS-ul a fost trimis cu succes</returns>
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        
        /// <summary>
        /// Genereaz? cod de verificare sigur de 6 cifre
        /// </summary>
        /// <returns>Cod de 6 cifre</returns>
        string GenerateVerificationCode();
        
        /// <summary>
        /// Verific? dac? un num?r de telefon este valid pentru România
        /// </summary>
        /// <param name="phoneNumber">Num?rul de telefon de verificat</param>
        /// <returns>True dac? num?rul este valid</returns>
        bool IsValidRomanianPhoneNumber(string phoneNumber);
        
        /// <summary>
        /// Normalizeaz? num?rul de telefon la formatul intern (07XXXXXXXX)
        /// </summary>
        /// <param name="phoneNumber">Num?rul de telefon de normalizat</param>
        /// <returns>Num?rul normalizat sau null dac? invalid</returns>
        string? NormalizePhoneNumber(string? phoneNumber);
    }
}
using RestSharp;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WebApplication1.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private static readonly Regex RomanianPhoneRegex = new(@"^(\+40|0040|40)?0?7[0-8][0-9]{7}$", RegexOptions.Compiled);

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetSmsAsync(string phoneNumber, string resetCode)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(resetCode))
            {
                throw new ArgumentException("Num?rul de telefon ?i codul sunt obligatorii");
            }

            var message = $"Miseda Inspect SRL - Codul pentru resetarea parolei: {resetCode}. Valid 10 minute. Nu împ?rt??i acest cod!";
            
            var success = await SendSmsAsync(phoneNumber, message);
            if (!success)
            {
                throw new InvalidOperationException("Eroare la trimiterea SMS-ului de resetare parol?");
            }
        }

        public async Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Num?rul de telefon ?i codul sunt obligatorii");
            }

            var message = $"Miseda Inspect SRL - Codul de verificare: {code}. Valid 10 minute.";
            
            var success = await SendSmsAsync(phoneNumber, message);
            if (!success)
            {
                throw new InvalidOperationException("Eroare la trimiterea codului de verificare");
            }
        }

        public string GenerateVerificationCode()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 900000 + 100000).ToString();
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Validare input
                if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
                {
                    _logger.LogError("SMS parameters invalid: phone={PhoneNumber}, message={HasMessage}", 
                        phoneNumber, !string.IsNullOrWhiteSpace(message));
                    return false;
                }

                // Formatare ?i validare num?r telefon
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                if (string.IsNullOrEmpty(formattedPhone))
                {
                    _logger.LogError("Invalid phone number format: {PhoneNumber}", phoneNumber);
                    return false;
                }

                // Validare lungime mesaj
                if (message.Length > 160)
                {
                    _logger.LogWarning("SMS message too long ({Length} chars), will be split: {PhoneNumber}", 
                        message.Length, formattedPhone);
                }

                var apiToken = _configuration["SmsSettings:ApiToken"];
                var smsApiUrl = _configuration["SmsSettings:ApiUrl"] ?? "https://www.smsadvert.ro/api/sms/";

                if (string.IsNullOrEmpty(apiToken))
                {
                    // Pentru dezvoltare - simuleaz? trimiterea SMS
                    _logger.LogInformation("SMS SIMULAT c?tre: {PhoneNumber}", formattedPhone);
                    _logger.LogInformation("Mesaj: {Message}", message);
                    
                    // Simuleaz? delay-ul API-ului real
                    await Task.Delay(Random.Shared.Next(500, 1500));
                    
                    return true;
                }

                _logger.LogInformation("Attempting to send SMS to: {PhoneNumber}", formattedPhone);

                var client = new RestClient(smsApiUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Authorization", $"Bearer {apiToken}");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("User-Agent", "MisedaInspect/1.0");

                var smsData = new
                {
                    phone = formattedPhone,
                    shortTextMessage = message,
                    sendAsShort = true,
                    sender = "MisedaInspect"
                };

                var jsonBody = JsonSerializer.Serialize(smsData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                request.AddStringBody(jsonBody, DataFormat.Json);

                _logger.LogInformation("Sending SMS request to: {ApiUrl}", smsApiUrl);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation("SMS sent successfully to: {PhoneNumber}", formattedPhone);
                    
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        _logger.LogDebug("SMS API Response: {Response}", response.Content);
                    }
                    
                    return true;
                }
                else
                {
                    _logger.LogError("SMS sending failed - Status: {StatusCode}, Error: {ErrorMessage}", 
                        response.StatusCode, response.ErrorMessage ?? response.Content);
                    
                    return false;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error sending SMS to: {PhoneNumber}", phoneNumber);
                return false;
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Timeout sending SMS to: {PhoneNumber}", phoneNumber);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending SMS to: {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        private string? FormatPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            // Elimin? spa?iile, cratimele ?i parantezele
            var cleaned = Regex.Replace(phoneNumber, @"[\s\-\(\)]+", "");
            
            // Verific? dac? num?rul respect? formatul românesc
            if (!RomanianPhoneRegex.IsMatch(cleaned))
            {
                return null;
            }
            
            // Extrage doar cifrele
            var digitsOnly = new string(cleaned.Where(char.IsDigit).ToArray());
            
            // Format: 07XXXXXXXX (format intern)
            if (digitsOnly.StartsWith("07") && digitsOnly.Length == 10)
            {
                return "+4" + digitsOnly; // +407XXXXXXXX pentru API
            }
            
            // Format: 407XXXXXXXX
            if (digitsOnly.StartsWith("407") && digitsOnly.Length == 12)
            {
                return "+" + digitsOnly; // +407XXXXXXXX
            }
            
            // Format: 40407XXXXXXXX (full international cu country code)
            if (digitsOnly.StartsWith("40407") && digitsOnly.Length == 14)
            {
                return "+" + digitsOnly.Substring(2); // +407XXXXXXXX
            }
            
            return null;
        }

        public bool IsValidRomanianPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(FormatPhoneNumber(phoneNumber));
        }

        public string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            var formatted = FormatPhoneNumber(phoneNumber);
            if (string.IsNullOrEmpty(formatted))
            {
                return null;
            }

            // Returneaz? în format intern: 07XXXXXXXX
            if (formatted.StartsWith("+407"))
            {
                return "0" + formatted.Substring(4);
            }

            return null;
        }
    }
}
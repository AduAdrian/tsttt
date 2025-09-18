using RestSharp;
using System.Text.Json;

namespace WebApplication1.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetSmsAsync(string phoneNumber, string resetCode)
        {
            var message = $"Miseda Inspect SRL - Codul tau pentru resetarea parolei este: {resetCode}. Acest cod expira in 10 minute.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var apiToken = _configuration["SmsSettings:ApiToken"];
                var smsApiUrl = _configuration["SmsSettings:ApiUrl"] ?? "https://www.smsadvert.ro/api/sms/";

                if (string.IsNullOrEmpty(apiToken))
                {
                    // Pentru dezvoltare - logam SMS-ul in loc sa-l trimitem
                    _logger.LogInformation($"SMS SIMULAT catre: {phoneNumber}");
                    _logger.LogInformation($"Mesaj: {message}");
                    return;
                }

                // Asiguram ca numarul de telefon incepe cu +40
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                
                _logger.LogInformation($"Attempting to send SMS to: {formattedPhone}");

                var client = new RestClient(smsApiUrl);
                
                var request = new RestRequest("", Method.Post);
                request.AddHeader("Authorization", apiToken);
                request.AddHeader("Content-Type", "application/json");

                var smsData = new
                {
                    phone = formattedPhone,
                    shortTextMessage = message,
                    sendAsShort = true
                };

                var jsonBody = JsonSerializer.Serialize(smsData);
                request.AddStringBody(jsonBody, DataFormat.Json);

                _logger.LogInformation($"Sending SMS request: {jsonBody}");

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation($"SMS trimis cu succes catre: {formattedPhone}");
                    _logger.LogInformation($"Response: {response.Content}");
                }
                else
                {
                    _logger.LogError($"Eroare la trimiterea SMS: {response.StatusCode} - {response.Content}");
                    throw new Exception($"Eroare SMS API: {response.StatusCode} - {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eroare la trimiterea SMS catre: {phoneNumber}");
                throw;
            }
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Eliminam spatiile si caracterele speciale
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Daca incepe cu 07, inlocuim cu +407
            if (cleaned.StartsWith("07"))
            {
                return "+4" + cleaned;
            }
            
            // Daca incepe cu 407, adaugam +
            if (cleaned.StartsWith("407"))
            {
                return "+" + cleaned;
            }
            
            // Daca incepe cu +40, il lasam asa
            if (phoneNumber.StartsWith("+40"))
            {
                return phoneNumber;
            }
            
            // Altfel, presupunem ca e numar romanesc si adaugam +40
            return "+40" + cleaned;
        }
    }
}
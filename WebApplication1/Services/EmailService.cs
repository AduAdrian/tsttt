using MailKit.Net.Smtp;
using MimeKit;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            var subject = "Resetare Parola - Miseda Inspect SRL";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>Miseda Inspect SRL</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Resetare Parola</p>
                    </div>
                    
                    <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; color: #333; margin-bottom: 20px;'>Salut,</p>
                        
                        <p style='font-size: 16px; color: #333; margin-bottom: 20px;'>
                            Ai solicitat resetarea parolei pentru contul tau din aplicatia de Notificari Clienti. 
                            Apasa pe butonul de mai jos pentru a continua:
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' 
                               style='background: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; display: inline-block;'>
                                Reseteaza Parola
                            </a>
                        </div>
                        
                        <p style='font-size: 14px; color: #666; margin-top: 30px;'>
                            Daca nu ai solicitat resetarea parolei, poti ignora acest email in siguranta.
                        </p>
                        
                        <p style='font-size: 14px; color: #666;'>
                            Acest link va expira in 1 ora din motive de securitate.
                        </p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        
                        <p style='font-size: 12px; color: #999; text-align: center;'>
                            © 2024 Miseda Inspect SRL - Sistem de Notificari Clienti<br>
                            Acest email a fost trimis automat, te rugam sa nu raspunzi direct.
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirmare Email - Miseda Inspect SRL";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0;'>
                        <h1 style='margin: 0; font-size: 28px;'>Miseda Inspect SRL</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Confirmare Email</p>
                    </div>
                    
                    <div style='background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; color: #333; margin-bottom: 20px;'>Bine ai venit!</p>
                        
                        <p style='font-size: 16px; color: #333; margin-bottom: 20px;'>
                            Multumim ca te-ai inregistrat la aplicatia noastra de Notificari Clienti. 
                            Pentru a completa inregistrarea si pentru a te putea conecta, te rugam sa confirmi adresa de email apasand pe butonul de mai jos:
                        </p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' 
                               style='background: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-size: 16px; display: inline-block;'>
                                Confirma Email
                            </a>
                        </div>
                        
                        <p style='font-size: 14px; color: #666; margin-top: 30px;'>
                            <strong>Important:</strong> Fara confirmarea emailului nu te vei putea conecta la cont.
                        </p>
                        
                        <p style='font-size: 14px; color: #666;'>
                            Daca nu ai creat acest cont, poti ignora acest email in siguranta.
                        </p>
                        
                        <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                        
                        <p style='font-size: 12px; color: #999; text-align: center;'>
                            © 2024 Miseda Inspect SRL - Sistem de Notificari Clienti<br>
                            Acest email a fost trimis automat, te rugam sa nu raspunzi direct.
                        </p>
                    </div>
                </div>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var mimeMessage = new MimeMessage();
                
                var fromName = _configuration["EmailSettings:FromName"] ?? "Miseda Inspect SRL";
                var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "notificari-sms@misedainspectsrl.ro";
                
                mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
                mimeMessage.To.Add(new MailboxAddress("", email));
                mimeMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = message;
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(smtpServer))
                {
                    // Pentru dezvoltare - logam emailul in loc sa-l trimitem
                    _logger.LogInformation($"EMAIL SIMULAT catre: {email}");
                    _logger.LogInformation($"Subiect: {subject}");
                    _logger.LogInformation($"Link confirmatie: {message}");
                    return;
                }

                _logger.LogInformation($"Attempting to send email to: {email}");
                _logger.LogInformation($"SMTP Server: {smtpServer}:{smtpPort}");
                _logger.LogInformation($"Username: {smtpUsername}");

                try
                {
                    // Incearca mai intai cu STARTTLS pe portul 587
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    _logger.LogInformation("Connected with STARTTLS on port 587");
                }
                catch (Exception ex1)
                {
                    _logger.LogWarning($"Failed to connect with STARTTLS on port 587: {ex1.Message}");
                    
                    try
                    {
                        // Incearca cu SSL pe portul 465
                        await client.ConnectAsync(smtpServer, 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                        _logger.LogInformation("Connected with SSL on port 465");
                    }
                    catch (Exception ex2)
                    {
                        _logger.LogWarning($"Failed to connect with SSL on port 465: {ex2.Message}");
                        
                        try
                        {
                            // Incearca fara SSL pe portul 587
                            await client.ConnectAsync(smtpServer, 587, MailKit.Security.SecureSocketOptions.None);
                            _logger.LogInformation("Connected without SSL on port 587");
                        }
                        catch (Exception ex3)
                        {
                            _logger.LogWarning($"Failed to connect without SSL on port 587: {ex3.Message}");
                            
                            // Ultima incercare pe portul 25 fara SSL
                            await client.ConnectAsync(smtpServer, 25, MailKit.Security.SecureSocketOptions.None);
                            _logger.LogInformation("Connected without SSL on port 25");
                        }
                    }
                }
                
                // Autentificare
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                _logger.LogInformation("Authentication successful");
                
                // Trimitere email
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email trimis cu succes catre: {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eroare la trimiterea emailului catre: {email}");
                _logger.LogError($"Full error details: {ex}");
                
                throw;
            }
        }
    }
}
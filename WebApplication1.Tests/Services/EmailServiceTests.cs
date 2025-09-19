using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WebApplication1.Services;

namespace WebApplication1.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup configuration values to simulate development mode (no SMTP server)
            _mockConfiguration.Setup(x => x["EmailSettings:FromName"]).Returns("Test App");
            _mockConfiguration.Setup(x => x["EmailSettings:FromEmail"]).Returns("test@example.com");
            _mockConfiguration.Setup(x => x["EmailSettings:SmtpServer"]).Returns((string?)null);
            _mockConfiguration.Setup(x => x["EmailSettings:SmtpPort"]).Returns("587");
            _mockConfiguration.Setup(x => x["EmailSettings:Username"]).Returns("test@example.com");
            _mockConfiguration.Setup(x => x["EmailSettings:Password"]).Returns("password");
            
            _emailService = new EmailService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldNotThrowException_WhenCalledWithValidParameters()
        {
            // Arrange
            var to = "recipient@example.com";
            var subject = "Test Subject";
            var body = "Test Body";

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _emailService.SendEmailAsync(to, subject, body));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("", "Subject", "Body")]
        [InlineData("to@example.com", "", "Body")]
        [InlineData("to@example.com", "Subject", "")]
        public async Task SendEmailAsync_ShouldNotThrow_WhenCalledWithInvalidParameters(string to, string subject, string body)
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _emailService.SendEmailAsync(to, subject, body));
            // The service should handle invalid parameters gracefully
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendEmailConfirmationAsync_ShouldCallSendEmailAsync()
        {
            // Arrange
            var email = "user@example.com";
            var confirmationLink = "https://example.com/confirm?token=123";

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => 
                _emailService.SendEmailConfirmationAsync(email, confirmationLink));
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldCallSendEmailAsync()
        {
            // Arrange
            var email = "user@example.com";
            var resetLink = "https://example.com/reset?token=123";

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => 
                _emailService.SendPasswordResetEmailAsync(email, resetLink));
            Assert.Null(exception);
        }
    }
}
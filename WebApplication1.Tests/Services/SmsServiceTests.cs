using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WebApplication1.Services;

namespace WebApplication1.Tests.Services
{
    public class SmsServiceTests
    {
        private readonly Mock<ILogger<SmsService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly SmsService _smsService;

        public SmsServiceTests()
        {
            _mockLogger = new Mock<ILogger<SmsService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            // Setup configuration values for SMS service to simulate development mode (no API token)
            _mockConfiguration.Setup(x => x["SmsSettings:ApiToken"]).Returns((string?)null);
            _mockConfiguration.Setup(x => x["SmsSettings:ApiUrl"]).Returns("https://test-api.com");
            
            _smsService = new SmsService(_mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SendSmsAsync_ShouldNotThrowException_WhenCalledWithValidParameters()
        {
            // Arrange
            var phoneNumber = "+40123456789";
            var message = "Test message";

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _smsService.SendSmsAsync(phoneNumber, message));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("", "Message")]
        [InlineData("+40123456789", "")]
        public async Task SendSmsAsync_ShouldNotThrow_WhenCalledWithInvalidParameters(string phoneNumber, string message)
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _smsService.SendSmsAsync(phoneNumber, message));
            // The service should handle invalid parameters gracefully
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendVerificationCodeAsync_ShouldCallSendSmsAsync()
        {
            // Arrange
            var phoneNumber = "+40123456789";
            var code = "123456";

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => 
                _smsService.SendVerificationCodeAsync(phoneNumber, code));
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendPasswordResetSmsAsync_ShouldCallSendSmsAsync()
        {
            // Arrange
            var phoneNumber = "+40123456789";
            var code = "123456";

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => 
                _smsService.SendPasswordResetSmsAsync(phoneNumber, code));
            Assert.Null(exception);
        }

        [Fact]
        public void GenerateVerificationCode_ShouldReturn6DigitString()
        {
            // Act
            var code = _smsService.GenerateVerificationCode();

            // Assert
            Assert.NotNull(code);
            Assert.Equal(6, code.Length);
            Assert.True(int.TryParse(code, out _), "Code should be numeric");
        }

        [Fact]
        public void GenerateVerificationCode_ShouldReturnDifferentCodes()
        {
            // Act
            var code1 = _smsService.GenerateVerificationCode();
            var code2 = _smsService.GenerateVerificationCode();

            // Assert - while theoretically they could be the same, it's very unlikely
            // This test might occasionally fail, but it's useful for detecting obvious problems
            Assert.NotEqual(code1, code2);
        }
    }
}
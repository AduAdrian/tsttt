using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class ClientTests
    {
        [Fact]
        public void Client_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var client = new Client();

            // Assert
            Assert.Equal(ValidityType.Manual, client.ValidityType);
            Assert.True(client.IsActive);
            Assert.True((DateTime.Now - client.CreatedAt).TotalSeconds < 1); // Created recently
            Assert.Equal(string.Empty, client.RegistrationNumber);
        }

        [Theory]
        [InlineData("B123ABC")]
        [InlineData("CJ456DEF")]
        [InlineData("BH789GHI")]
        public void Client_RegistrationNumber_AcceptsValidFormats(string registrationNumber)
        {
            // Arrange & Act
            var client = new Client { RegistrationNumber = registrationNumber };

            // Assert
            Assert.Equal(registrationNumber, client.RegistrationNumber);
        }

        [Theory]
        [InlineData(ValidityType.Manual, 0)]
        [InlineData(ValidityType.SixMonths, 6)]
        [InlineData(ValidityType.OneYear, 12)]
        [InlineData(ValidityType.TwoYears, 24)]
        public void ValidityType_EnumValues_HaveCorrectIntegerValues(ValidityType validityType, int expectedValue)
        {
            // Assert
            Assert.Equal(expectedValue, (int)validityType);
        }

        [Fact]
        public void Client_ExpiryDate_CanBeSetToFutureDate()
        {
            // Arrange
            var futureDate = DateTime.Today.AddMonths(6);
            var client = new Client();

            // Act
            client.ExpiryDate = futureDate;

            // Assert
            Assert.Equal(futureDate, client.ExpiryDate);
        }
    }

    public class ClientViewModelsTests
    {
        [Fact]
        public void ClientCreateViewModel_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var viewModel = new ClientCreateViewModel();

            // Assert
            Assert.Equal(ValidityType.Manual, viewModel.ValidityType); // Default este Manual
            Assert.Equal(string.Empty, viewModel.RegistrationNumber);
        }

        [Theory]
        [InlineData("", false)] // Empty registration number should be invalid
        [InlineData("B123ABC", true)] // Valid registration number
        [InlineData("TOOLONGREGISTRATIONNUMBER", false)] // Too long (>15 chars)
        public void ClientCreateViewModel_RegistrationNumber_Validation(string registrationNumber, bool shouldBeValid)
        {
            // Arrange
            var viewModel = new ClientCreateViewModel
            {
                RegistrationNumber = registrationNumber,
                ValidityType = ValidityType.Manual,
                ExpiryDate = DateTime.Today.AddDays(30)
            };

            // Act
            var validationResults = ValidateModel(viewModel);

            // Assert
            var hasRegistrationErrors = validationResults.Any(v => v.MemberNames.Contains(nameof(viewModel.RegistrationNumber)));
            Assert.Equal(!shouldBeValid, hasRegistrationErrors);
        }

        [Fact]
        public void ClientListViewModel_StatusProperties_CalculateCorrectly()
        {
            // Test pentru client care expir? în 15 zile (expiringSoon)
            var expiringSoonClient = new ClientListViewModel
            {
                ExpiryDate = DateTime.Today.AddDays(15),
                IsActive = true
            };

            // Test pentru client expirat
            var expiredClient = new ClientListViewModel
            {
                ExpiryDate = DateTime.Today.AddDays(-5),
                IsActive = true
            };

            // Test pentru client valid
            var validClient = new ClientListViewModel
            {
                ExpiryDate = DateTime.Today.AddDays(60),
                IsActive = true
            };

            // Assert pentru expiringSoon
            Assert.True(expiringSoonClient.IsExpiringSoon);
            Assert.False(expiringSoonClient.IsExpired);
            Assert.Equal("badge bg-warning", expiringSoonClient.StatusBadgeClass);
            Assert.Equal("Expira curând", expiringSoonClient.StatusText);

            // Assert pentru expired
            Assert.True(expiredClient.IsExpired);
            Assert.False(expiredClient.IsExpiringSoon);
            Assert.Equal("badge bg-danger", expiredClient.StatusBadgeClass);
            Assert.Equal("Expirat", expiredClient.StatusText);

            // Assert pentru valid
            Assert.False(validClient.IsExpired);
            Assert.False(validClient.IsExpiringSoon);
            Assert.Equal("badge bg-success", validClient.StatusBadgeClass);
            Assert.Equal("Valid", validClient.StatusText);
        }

        [Theory]
        [InlineData(15, 15)] // 15 zile pân? la expirare
        [InlineData(-5, -5)] // 5 zile întârziere (negativ)
        [InlineData(0, 0)] // Expir? azi
        public void ClientListViewModel_DaysUntilExpiry_CalculatesCorrectly(int daysFromToday, int expectedDays)
        {
            // Arrange
            var viewModel = new ClientListViewModel
            {
                ExpiryDate = DateTime.Today.AddDays(daysFromToday)
            };

            // Act & Assert
            Assert.Equal(expectedDays, viewModel.DaysUntilExpiry);
        }

        [Theory]
        [InlineData(ValidityType.Manual, "Manual")]
        [InlineData(ValidityType.SixMonths, "6 Luni")]
        [InlineData(ValidityType.OneYear, "1 An")]
        [InlineData(ValidityType.TwoYears, "2 Ani")]
        public void ValidityType_DisplayNames_AreCorrect(ValidityType validityType, string expectedDisplayName)
        {
            // Act
            var displayName = validityType.GetDisplayName();

            // Assert
            Assert.Equal(expectedDisplayName, displayName);
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
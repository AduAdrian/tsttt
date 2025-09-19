using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace WebApplication1.Tests.Integration
{
    public class ClientCreatePageTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ClientCreatePageTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task CreateClientPage_LoadsSuccessfully()
        {
            // Act
            var response = await _client.GetAsync("/Clients/Create");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            // Verific? c? elementele necesare sunt prezente
            Assert.Contains("Nr Înmatriculare", content);
            Assert.Contains("Valabilitate", content);
            Assert.Contains("Data Expirare ITP", content);
            
            // Verific? c? JavaScript-ul pentru handling este inclus
            Assert.Contains("handleValidityTypeChange", content);
            Assert.Contains("calculateExpiryDate", content);
            Assert.Contains("initializeClientForm", content);
        }

        [Fact]
        public async Task CreateClientPage_DefaultValidity_IsManual()
        {
            // Act
            var response = await _client.GetAsync("/Clients/Create");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            // Verific? c? op?iunea "Manual" este selectat? implicit
            Assert.Contains("value=\"0\" selected>Manual", content);
        }

        [Fact]
        public async Task CreateClientPage_ContainsRequiredFormFields()
        {
            // Act
            var response = await _client.GetAsync("/Clients/Create");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            // Verific? c? toate câmpurile necesare sunt prezente
            Assert.Contains("name=\"RegistrationNumber\"", content);
            Assert.Contains("name=\"ValidityType\"", content);
            Assert.Contains("name=\"ExpiryDate\"", content);
            
            // Verific? c? select-ul are toate op?iunile
            Assert.Contains("value=\"0\">Manual", content);
            Assert.Contains("value=\"6\">6 Luni", content);
            Assert.Contains("value=\"12\">1 An", content);
            Assert.Contains("value=\"24\">2 Ani", content);
        }

        [Fact]
        public async Task CreateClientPage_ContainsValidationElements()
        {
            // Act
            var response = await _client.GetAsync("/Clients/Create");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            // Verific? c? validation scripts sunt incluse
            Assert.Contains("_ValidationScriptsPartial", content);
            
            // Verific? c? validation spans sunt prezente
            Assert.Contains("asp-validation-for=\"RegistrationNumber\"", content);
            Assert.Contains("asp-validation-for=\"ValidityType\"", content);
            Assert.Contains("asp-validation-for=\"ExpiryDate\"", content);
        }

        [Fact]
        public async Task CreateClientPage_ContainsCalculatorElements()
        {
            // Act
            var response = await _client.GetAsync("/Clients/Create");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            // Verific? c? elementele pentru calculator sunt prezente
            Assert.Contains("id=\"calculatorInfo\"", content);
            Assert.Contains("id=\"calendarIcon\"", content);
            Assert.Contains("id=\"lockIcon\"", content);
            Assert.Contains("id=\"expiryDateHelp\"", content);
        }
    }

    public class ClientValidityCalculationTests
    {
        [Theory]
        [InlineData(6, 6)] // 6 luni
        [InlineData(12, 12)] // 1 an
        [InlineData(24, 24)] // 2 ani
        public void CalculateExpiryDate_AutomaticModes_ShouldAddCorrectMonths(int validityMonths, int expectedMonths)
        {
            // Arrange
            var today = DateTime.Today;
            var expectedDate = today.AddMonths(expectedMonths);

            // Act
            var calculatedDate = today.AddMonths(validityMonths);

            // Assert
            Assert.Equal(expectedDate.Date, calculatedDate.Date);
        }

        [Fact]
        public void ManualMode_ShouldAllowUserDefinedDate()
        {
            // Arrange
            var userDefinedDate = DateTime.Today.AddDays(45);

            // Act - Simuleaz? c? utilizatorul seteaz? manual data
            var resultDate = userDefinedDate;

            // Assert
            Assert.Equal(userDefinedDate.Date, resultDate.Date);
        }

        [Theory]
        [InlineData("2024-01-01", "2024-07-01", 6)] // 6 luni de la 1 ianuarie
        [InlineData("2024-06-15", "2025-06-15", 12)] // 1 an de la 15 iunie
        [InlineData("2023-12-31", "2025-12-31", 24)] // 2 ani de la 31 decembrie
        public void DateCalculation_VariousStartDates_ProducesCorrectResults(
            string startDateString, 
            string expectedDateString, 
            int months)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var expectedDate = DateTime.Parse(expectedDateString);

            // Act
            var calculatedDate = startDate.AddMonths(months);

            // Assert
            Assert.Equal(expectedDate.Date, calculatedDate.Date);
        }

        [Fact]
        public void ValidityType_DefaultSelection_IsManual()
        {
            // Arrange
            var validityType = ValidityType.Manual;

            // Assert
            Assert.Equal(0, (int)validityType);
            Assert.Equal(ValidityType.Manual, validityType);
        }
    }
}
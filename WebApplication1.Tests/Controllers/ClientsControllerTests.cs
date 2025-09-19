using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Controllers
{
    public class ClientsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClientsController _controller;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public ClientsControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup Mock UserManager
            var mockStore = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                mockStore.Object, null, null, null, null, null, null, null, null);

            _controller = new ClientsController(_context, _mockUserManager.Object);
        }

        [Fact]
        public async Task Create_GET_ReturnsViewWithCorrectDefaults()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ClientCreateViewModel>(viewResult.Model);
            
            // Verific? c? default-ul este Manual
            Assert.Equal(ValidityType.Manual, model.ValidityType);
            Assert.Equal(string.Empty, model.RegistrationNumber);
        }

        [Theory]
        [InlineData("B123ABC", ValidityType.Manual, "2024-12-31")] // Manual - user sets date
        [InlineData("CJ456DEF", ValidityType.SixMonths, null)] // 6 months - auto calculate
        [InlineData("BH789GHI", ValidityType.OneYear, null)] // 1 year - auto calculate  
        [InlineData("IS012JKL", ValidityType.TwoYears, null)] // 2 years - auto calculate
        public async Task Create_POST_ValidModel_CreatesClientWithCorrectData(
            string registrationNumber, 
            ValidityType validityType, 
            string? manualDateString)
        {
            // Arrange
            var expectedDate = manualDateString != null 
                ? DateTime.Parse(manualDateString)
                : CalculateExpectedExpiryDate(validityType);

            var model = new ClientCreateViewModel
            {
                RegistrationNumber = registrationNumber,
                ValidityType = validityType,
                ExpiryDate = expectedDate
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verific? c? clientul a fost creat în baza de date
            var createdClient = await _context.Clients.FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);
            Assert.NotNull(createdClient);
            Assert.Equal(registrationNumber, createdClient.RegistrationNumber);
            Assert.Equal(validityType, createdClient.ValidityType);
            Assert.Equal(expectedDate.Date, createdClient.ExpiryDate.Date);
            Assert.Equal("test-user-id", createdClient.CreatedByUserId);
            Assert.True(createdClient.IsActive);
        }

        [Fact]
        public async Task Create_POST_ManualMode_AcceptsUserDefinedDate()
        {
            // Arrange - Manual mode cu data setat? de utilizator
            var userDefinedDate = DateTime.Today.AddDays(45); // Utilizatorul alege o dat?
            var model = new ClientCreateViewModel
            {
                RegistrationNumber = "B999MAN",
                ValidityType = ValidityType.Manual,
                ExpiryDate = userDefinedDate
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var createdClient = await _context.Clients.FirstOrDefaultAsync(c => c.RegistrationNumber == "B999MAN");
            Assert.NotNull(createdClient);
            Assert.Equal(ValidityType.Manual, createdClient.ValidityType);
            Assert.Equal(userDefinedDate.Date, createdClient.ExpiryDate.Date);
        }

        [Theory]
        [InlineData(ValidityType.SixMonths, 6)]
        [InlineData(ValidityType.OneYear, 12)]
        [InlineData(ValidityType.TwoYears, 24)]
        public void CalculateExpectedExpiryDate_AutomaticModes_ReturnsCorrectDate(ValidityType validityType, int expectedMonths)
        {
            // Arrange
            var today = DateTime.Today;
            
            // Act
            var result = CalculateExpectedExpiryDate(validityType);
            var expectedDate = today.AddMonths(expectedMonths);

            // Assert
            Assert.Equal(expectedDate.Date, result.Date);
        }

        [Fact]
        public async Task Create_POST_DuplicateRegistrationNumber_ReturnsViewWithError()
        {
            // Arrange - Adaug? un client existent
            var existingClient = new Client
            {
                RegistrationNumber = "DUPLICATE123",
                ValidityType = ValidityType.Manual,
                ExpiryDate = DateTime.Today.AddDays(30),
                IsActive = true
            };
            _context.Clients.Add(existingClient);
            await _context.SaveChangesAsync();

            var duplicateModel = new ClientCreateViewModel
            {
                RegistrationNumber = "DUPLICATE123",
                ValidityType = ValidityType.SixMonths,
                ExpiryDate = DateTime.Today.AddMonths(6)
            };

            // Act
            var result = await _controller.Create(duplicateModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(duplicateModel.RegistrationNumber)));
        }

        [Theory]
        [InlineData("", ValidityType.Manual)] // Empty registration number
        [InlineData("TOOLONGREGISTRATIONNUMBER", ValidityType.Manual)] // Too long registration number
        public async Task Create_POST_InvalidModel_ReturnsViewWithErrors(string registrationNumber, ValidityType validityType)
        {
            // Arrange
            var invalidModel = new ClientCreateViewModel
            {
                RegistrationNumber = registrationNumber,
                ValidityType = validityType,
                ExpiryDate = DateTime.Today.AddDays(30)
            };

            // Act
            if (string.IsNullOrEmpty(registrationNumber))
            {
                _controller.ModelState.AddModelError(nameof(invalidModel.RegistrationNumber), "Required");
            }
            if (registrationNumber.Length > 15)
            {
                _controller.ModelState.AddModelError(nameof(invalidModel.RegistrationNumber), "Too long");
            }

            var result = await _controller.Create(invalidModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void ValidityType_DefaultValue_IsManual()
        {
            // Arrange & Act
            var viewModel = new ClientCreateViewModel();

            // Assert
            Assert.Equal(ValidityType.Manual, viewModel.ValidityType);
        }

        [Fact]
        public void ValidityType_EnumValues_AreCorrect()
        {
            // Assert
            Assert.Equal(0, (int)ValidityType.Manual);
            Assert.Equal(6, (int)ValidityType.SixMonths);
            Assert.Equal(12, (int)ValidityType.OneYear);
            Assert.Equal(24, (int)ValidityType.TwoYears);
        }

        private DateTime CalculateExpectedExpiryDate(ValidityType validityType)
        {
            var today = DateTime.Today;
            return validityType switch
            {
                ValidityType.SixMonths => today.AddMonths(6),
                ValidityType.OneYear => today.AddMonths(12),
                ValidityType.TwoYears => today.AddMonths(24),
                ValidityType.Manual => today, // Manual mode - date should be user-defined
                _ => today
            };
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
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
    public class ClientsControllerDuplicateTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClientsController _controller;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

        public ClientsControllerDuplicateTests()
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
        public async Task Create_DuplicateActiveClient_ReturnsViewWithError()
        {
            // Arrange - Add an existing active client
            var existingClient = new Client
            {
                RegistrationNumber = "B123ABC",
                ValidityType = ValidityType.SixMonths,
                ExpiryDate = DateTime.Today.AddMonths(6),
                IsActive = true
            };
            _context.Clients.Add(existingClient);
            await _context.SaveChangesAsync();

            var duplicateModel = new ClientCreateViewModel
            {
                RegistrationNumber = "b123abc", // Different case - should still be caught
                ValidityType = ValidityType.OneYear,
                ExpiryDate = DateTime.Today.AddMonths(12)
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(duplicateModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("client activ cu acest num?r", _controller.ModelState[nameof(duplicateModel.RegistrationNumber)]?.Errors.First().ErrorMessage);

            // Verify no new client was added
            var clientCount = await _context.Clients.CountAsync();
            Assert.Equal(1, clientCount);
        }

        [Fact]
        public async Task Create_DuplicateInactiveClient_ReactivatesClient()
        {
            // Arrange - Add an existing inactive client
            var inactiveClient = new Client
            {
                RegistrationNumber = "CJ456DEF",
                ValidityType = ValidityType.SixMonths,
                ExpiryDate = DateTime.Today.AddMonths(6),
                IsActive = false // Inactive client
            };
            _context.Clients.Add(inactiveClient);
            await _context.SaveChangesAsync();

            var reactivateModel = new ClientCreateViewModel
            {
                RegistrationNumber = "cj456def", // Different case - should still match
                ValidityType = ValidityType.OneYear,
                ExpiryDate = DateTime.Today.AddMonths(12)
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(reactivateModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify client was reactivated with new data
            var reactivatedClient = await _context.Clients.FirstOrDefaultAsync(c => c.RegistrationNumber == "CJ456DEF");
            Assert.NotNull(reactivatedClient);
            Assert.True(reactivatedClient.IsActive);
            Assert.Equal(ValidityType.OneYear, reactivatedClient.ValidityType);
            Assert.Equal("test-user-id", reactivatedClient.CreatedByUserId);

            // Verify only one client exists (reactivated, not new)
            var clientCount = await _context.Clients.CountAsync();
            Assert.Equal(1, clientCount);
        }

        [Theory]
        [InlineData("B123ABC", "b123abc")] // Different case
        [InlineData("CJ456DEF", "  cj456def  ")] // With spaces
        [InlineData("BH789GHI", "BH789GHI")] // Exact match
        public async Task Create_RegistrationNumberNormalization_WorksCorrectly(string storedNumber, string inputNumber)
        {
            // Arrange - Add an existing active client
            var existingClient = new Client
            {
                RegistrationNumber = storedNumber,
                ValidityType = ValidityType.SixMonths,
                ExpiryDate = DateTime.Today.AddMonths(6),
                IsActive = true
            };
            _context.Clients.Add(existingClient);
            await _context.SaveChangesAsync();

            var duplicateModel = new ClientCreateViewModel
            {
                RegistrationNumber = inputNumber,
                ValidityType = ValidityType.OneYear,
                ExpiryDate = DateTime.Today.AddMonths(12)
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(duplicateModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            
            // Verify error message exists
            var registrationErrors = _controller.ModelState[nameof(duplicateModel.RegistrationNumber)]?.Errors;
            Assert.NotNull(registrationErrors);
            Assert.NotEmpty(registrationErrors);
        }

        [Fact]
        public async Task Create_NewRegistrationNumber_CreatesSuccessfully()
        {
            // Arrange
            var newModel = new ClientCreateViewModel
            {
                RegistrationNumber = "NEW123ABC",
                ValidityType = ValidityType.Manual,
                ExpiryDate = DateTime.Today.AddDays(90)
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(newModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify client was created
            var createdClient = await _context.Clients.FirstOrDefaultAsync(c => c.RegistrationNumber == "NEW123ABC");
            Assert.NotNull(createdClient);
            Assert.True(createdClient.IsActive);
            Assert.Equal(ValidityType.Manual, createdClient.ValidityType);
            Assert.Equal("test-user-id", createdClient.CreatedByUserId);
        }

        [Theory]
        [InlineData(ValidityType.SixMonths, 6)]
        [InlineData(ValidityType.OneYear, 12)]
        [InlineData(ValidityType.TwoYears, 24)]
        public async Task Create_AutomaticValidityTypes_CalculateCorrectExpiryDate(ValidityType validityType, int expectedMonths)
        {
            // Arrange
            var today = DateTime.Today;
            var model = new ClientCreateViewModel
            {
                RegistrationNumber = $"AUTO{(int)validityType}",
                ValidityType = validityType,
                ExpiryDate = DateTime.Today.AddDays(1) // This should be overridden by auto calculation
            };

            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                           .Returns("test-user-id");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify client was created with correct calculated date
            var createdClient = await _context.Clients.FirstOrDefaultAsync(c => c.RegistrationNumber == $"AUTO{(int)validityType}");
            Assert.NotNull(createdClient);
            
            var expectedDate = today.AddMonths(expectedMonths);
            Assert.Equal(expectedDate.Date, createdClient.ExpiryDate.Date);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Tests.Controllers
{
    public class AppointmentsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<AppointmentsController>> _mockLogger;
        private readonly AppointmentsController _controller;

        public AppointmentsControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Ensure database is created
            _context.Database.EnsureCreated();

            // Setup UserManager mock
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            
            _mockLogger = new Mock<ILogger<AppointmentsController>>();
            
            _controller = new AppointmentsController(_context, _mockUserManager.Object, _mockLogger.Object);
            
            // Setup user context
            SetupControllerContext();
        }

        private void SetupControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            var testUser = new ApplicationUser { Id = "1", UserName = "testuser", Email = "test@test.com" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithModel()
        {
            // Act
            var result = await _controller.Index(null, null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AppointmentListViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task Create_Get_ShouldReturnViewWithModel()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CreateAppointmentViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_ShouldRedirectToDetails()
        {
            // Arrange
            var model = new CreateAppointmentViewModel
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60
            };

            // Setup TempData mock
            var tempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(), 
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
            _controller.TempData = tempData;

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.NotNull(redirectResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ShouldReturnView()
        {
            // Arrange
            var model = new CreateAppointmentViewModel(); // Invalid - missing required fields
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
        }

        [Fact]
        public async Task Details_ValidId_ShouldReturnViewWithModel()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60,
                CreatedByUserId = "1",
                CreatedAt = DateTime.Now
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(appointment.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AppointmentDetailsViewModel>(viewResult.Model);
            Assert.Equal(appointment.Id, model.Appointment.Id);
        }

        [Fact]
        public async Task Details_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ValidId_ShouldReturnViewWithModel()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60,
                CreatedByUserId = "1",
                CreatedAt = DateTime.Now
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Edit(appointment.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditAppointmentViewModel>(viewResult.Model);
            Assert.Equal(appointment.Id, model.Id);
        }

        [Fact]
        public async Task Edit_InvalidId_ShouldReturnNotFound()
        {
            // Act
            var result = await _controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Calendar_ShouldReturnViewWithModel()
        {
            // Act
            var result = await _controller.Calendar(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AppointmentCalendarViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public async Task UpdateStatus_ValidId_ShouldReturnJsonSuccess()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60,
                Status = AppointmentStatus.Scheduled,
                CreatedByUserId = "1",
                CreatedAt = DateTime.Now
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.UpdateStatus(appointment.Id, AppointmentStatus.Confirmed);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            Assert.NotNull(resultValue);
            
            // Use reflection to check the anonymous object properties
            var successProperty = resultValue.GetType().GetProperty("success");
            var messageProperty = resultValue.GetType().GetProperty("message");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(messageProperty);
            Assert.True((bool)successProperty.GetValue(resultValue)!);
        }

        [Fact]
        public async Task UpdateStatus_InvalidId_ShouldReturnJsonError()
        {
            // Act
            var result = await _controller.UpdateStatus(999, AppointmentStatus.Confirmed);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            Assert.NotNull(resultValue);
            
            // Use reflection to check the anonymous object properties
            var successProperty = resultValue.GetType().GetProperty("success");
            var messageProperty = resultValue.GetType().GetProperty("message");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(messageProperty);
            Assert.False((bool)successProperty.GetValue(resultValue)!);
        }

        [Fact]
        public async Task Delete_ValidId_ShouldReturnJsonSuccess()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Test Appointment",
                ClientName = "Test Client",
                AppointmentDate = DateTime.Now.AddHours(2),
                DurationMinutes = 60,
                CreatedByUserId = "1",
                CreatedAt = DateTime.Now
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(appointment.Id);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            Assert.NotNull(resultValue);
            
            // Use reflection to check the anonymous object properties
            var successProperty = resultValue.GetType().GetProperty("success");
            var messageProperty = resultValue.GetType().GetProperty("message");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(messageProperty);
            Assert.True((bool)successProperty.GetValue(resultValue)!);

            // Verify appointment was deleted
            var deletedAppointment = await _context.Appointments.FindAsync(appointment.Id);
            Assert.Null(deletedAppointment);
        }

        [Fact]
        public async Task Delete_InvalidId_ShouldReturnJsonError()
        {
            // Act
            var result = await _controller.Delete(999);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            Assert.NotNull(resultValue);
            
            // Use reflection to check the anonymous object properties
            var successProperty = resultValue.GetType().GetProperty("success");
            var messageProperty = resultValue.GetType().GetProperty("message");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(messageProperty);
            Assert.False((bool)successProperty.GetValue(resultValue)!);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
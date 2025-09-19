using Xunit;
using WebApplication1.Models;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Tests.Models
{
    public class AppointmentViewModelsTests
    {
        [Fact]
        public void CreateAppointmentViewModel_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act
            var model = new CreateAppointmentViewModel();

            // Assert
            Assert.Equal(60, model.DurationMinutes);
            Assert.Equal(AppointmentStatus.Scheduled, model.Status);
            Assert.True(model.AppointmentDate >= DateTime.Today);
        }

        [Fact]
        public void CreateAppointmentViewModel_ShouldValidateRequiredFields()
        {
            // Arrange
            var model = new CreateAppointmentViewModel
            {
                // Leaving required fields empty
                Title = "",
                ClientName = ""
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientName"));
        }

        [Fact]
        public void CreateAppointmentViewModel_ShouldValidateEmailFormat()
        {
            // Arrange
            var model = new CreateAppointmentViewModel
            {
                Title = "Valid Title",
                ClientName = "Valid Name",
                ClientEmail = "invalid-email"
            };

            // Act
            var validationResults = ValidateModel(model);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientEmail"));
        }

        [Fact]
        public void AppointmentListViewModel_ShouldInitializeCollections()
        {
            // Arrange & Act
            var model = new AppointmentListViewModel();

            // Assert
            Assert.NotNull(model.Appointments);
            Assert.Empty(model.Appointments);
        }

        [Fact]
        public void AppointmentDetailsViewModel_ShouldInitializeCollections()
        {
            // Arrange & Act
            var model = new AppointmentDetailsViewModel();

            // Assert
            Assert.NotNull(model.Appointment);
            Assert.NotNull(model.History);
            Assert.Empty(model.History);
        }

        [Fact]
        public void CalendarDay_ShouldCalculateAppointmentCount()
        {
            // Arrange
            var appointment1 = new Appointment { Title = "Test 1", ClientName = "Client 1" };
            var appointment2 = new Appointment { Title = "Test 2", ClientName = "Client 2" };
            
            var calendarDay = new CalendarDay
            {
                Date = DateTime.Today,
                Appointments = new List<Appointment> { appointment1, appointment2 }
            };

            // Act & Assert
            Assert.Equal(2, calendarDay.AppointmentCount);
        }

        [Fact]
        public void AppointmentCalendarViewModel_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var model = new AppointmentCalendarViewModel();

            // Assert
            Assert.NotNull(model.Appointments);
            Assert.NotNull(model.Days);
            Assert.Empty(model.Appointments);
            Assert.Empty(model.Days);
        }

        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
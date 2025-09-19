using Xunit;
using WebApplication1.Models;

namespace WebApplication1.Tests.Models
{
    public class AppointmentTests
    {
        [Fact]
        public void Appointment_ShouldHaveCorrectDefaults()
        {
            // Arrange & Act
            var appointment = new Appointment();

            // Assert
            Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
            Assert.Equal(60, appointment.DurationMinutes);
            Assert.True(appointment.CreatedAt <= DateTime.Now);
            Assert.True(appointment.CreatedAt >= DateTime.Now.AddMinutes(-1));
        }

        [Fact]
        public void EndTime_ShouldCalculateCorrectly()
        {
            // Arrange
            var startTime = new DateTime(2024, 1, 15, 10, 30, 0);
            var appointment = new Appointment
            {
                AppointmentDate = startTime,
                DurationMinutes = 90
            };

            // Act
            var endTime = appointment.EndTime;

            // Assert
            Assert.Equal(new DateTime(2024, 1, 15, 12, 0, 0), endTime);
        }

        [Fact]
        public void IsToday_ShouldReturnTrueWhenAppointmentIsToday()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Today.AddHours(14)
            };

            // Act & Assert
            Assert.True(appointment.IsToday);
        }

        [Fact]
        public void IsToday_ShouldReturnFalseWhenAppointmentIsNotToday()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Today.AddDays(1).AddHours(14)
            };

            // Act & Assert
            Assert.False(appointment.IsToday);
        }

        [Fact]
        public void IsUpcoming_ShouldReturnTrueWhenAppointmentIsInFuture()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Now.AddHours(2)
            };

            // Act & Assert
            Assert.True(appointment.IsUpcoming);
        }

        [Fact]
        public void IsUpcoming_ShouldReturnFalseWhenAppointmentIsInPast()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Now.AddHours(-2)
            };

            // Act & Assert
            Assert.False(appointment.IsUpcoming);
        }

        [Fact]
        public void IsPast_ShouldReturnTrueWhenAppointmentIsCompletelyInPast()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Now.AddHours(-2),
                DurationMinutes = 60
            };

            // Act & Assert
            Assert.True(appointment.IsPast);
        }

        [Fact]
        public void IsPast_ShouldReturnFalseWhenAppointmentIsStillOngoing()
        {
            // Arrange
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Now.AddMinutes(-30),
                DurationMinutes = 60
            };

            // Act & Assert
            Assert.False(appointment.IsPast);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Title_ShouldBeRequired(string title)
        {
            // Arrange
            var appointment = new Appointment { Title = title };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ClientName_ShouldBeRequired(string clientName)
        {
            // Arrange
            var appointment = new Appointment 
            { 
                Title = "Valid Title",
                ClientName = clientName 
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientName"));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        public void ClientEmail_ShouldValidateEmailFormat(string email)
        {
            // Arrange
            var appointment = new Appointment 
            { 
                Title = "Valid Title",
                ClientName = "Valid Name",
                ClientEmail = email 
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientEmail"));
        }

        [Fact]
        public void DurationMinutes_ShouldBeInValidRange()
        {
            // Arrange
            var appointment = new Appointment 
            { 
                Title = "Valid Title",
                ClientName = "Valid Name",
                DurationMinutes = 10 // Below minimum
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("DurationMinutes"));
        }

        private List<System.ComponentModel.DataAnnotations.ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var ctx = new System.ComponentModel.DataAnnotations.ValidationContext(model, null, null);
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}
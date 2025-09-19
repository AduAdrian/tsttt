using Xunit;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Tests.Validation
{
    public class ModelValidationTests
    {
        [Fact]
        public void Appointment_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Appointment",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                ClientEmail = "john@example.com",
                ClientPhone = "+40123456789",
                Location = "Office",
                Notes = "Important meeting"
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Empty(validationResults);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Appointment_WithInvalidTitle_ShouldFailValidation(string title)
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = title,
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
        }

        [Fact]
        public void Appointment_WithTitleTooLong_ShouldFailValidation()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = new string('A', 201), // 201 characters, exceeds 200 limit
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Title"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Appointment_WithInvalidClientName_ShouldFailValidation(string clientName)
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = clientName,
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientName"));
        }

        [Theory]
        [InlineData(10)] // Below minimum
        [InlineData(500)] // Above maximum
        public void Appointment_WithInvalidDuration_ShouldFailValidation(int duration)
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = duration
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("DurationMinutes"));
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("@example.com")]
        [InlineData("user@")]
        [InlineData("user.example.com")]
        public void Appointment_WithInvalidEmail_ShouldFailValidation(string email)
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                ClientEmail = email
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientEmail"));
        }

        [Theory]
        [InlineData("john@example.com")]
        [InlineData("test.email+tag@example.co.uk")]
        [InlineData("user123@domain-name.com")]
        public void Appointment_WithValidEmail_ShouldPassValidation(string email)
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                ClientEmail = email
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.DoesNotContain(validationResults, v => v.MemberNames.Contains("ClientEmail"));
        }

        [Fact]
        public void Appointment_WithDescriptionTooLong_ShouldFailValidation()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                Description = new string('A', 1001) // 1001 characters, exceeds 1000 limit
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
        }

        [Fact]
        public void Appointment_WithPhoneTooLong_ShouldFailValidation()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                ClientPhone = new string('1', 16) // 16 characters, exceeds 15 limit
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("ClientPhone"));
        }

        [Fact]
        public void Appointment_WithLocationTooLong_ShouldFailValidation()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                Location = new string('A', 201) // 201 characters, exceeds 200 limit
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Location"));
        }

        [Fact]
        public void Appointment_WithNotesTooLong_ShouldFailValidation()
        {
            // Arrange
            var appointment = new Appointment
            {
                Title = "Valid Title",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                Notes = new string('A', 501) // 501 characters, exceeds 500 limit
            };

            // Act
            var validationResults = ValidateModel(appointment);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Notes"));
        }

        [Fact]
        public void CreateAppointmentViewModel_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var viewModel = new CreateAppointmentViewModel
            {
                Title = "Valid Appointment",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                ClientEmail = "john@example.com",
                ClientPhone = "+40123456789",
                Location = "Office"
            };

            // Act
            var validationResults = ValidateModel(viewModel);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void EditAppointmentViewModel_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var viewModel = new EditAppointmentViewModel
            {
                Id = 1,
                Title = "Valid Appointment",
                ClientName = "John Doe",
                AppointmentDate = DateTime.Now.AddDays(1),
                DurationMinutes = 60,
                ClientEmail = "john@example.com",
                CreatedAt = DateTime.Now.AddDays(-1)
            };

            // Act
            var validationResults = ValidateModel(viewModel);

            // Assert
            Assert.Empty(validationResults);
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
using Xunit;
using WebApplication1.Models;

namespace WebApplication1.Tests.Models
{
    public class EnumExtensionsTests
    {
        [Theory]
        [InlineData(AppointmentStatus.Scheduled, "Programat")]
        [InlineData(AppointmentStatus.Confirmed, "Confirmat")]
        [InlineData(AppointmentStatus.InProgress, "În desf??urare")]
        [InlineData(AppointmentStatus.Completed, "Finalizat")]
        [InlineData(AppointmentStatus.Cancelled, "Anulat")]
        [InlineData(AppointmentStatus.Rescheduled, "Reprogramat")]
        public void GetDisplayName_ShouldReturnCorrectDisplayName(AppointmentStatus status, string expectedName)
        {
            // Act
            var displayName = status.GetDisplayName();

            // Assert
            Assert.Equal(expectedName, displayName);
        }
    }
}
using Xunit;
using WebApplication1.Models;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Tests.Models
{
    public class ApplicationUserTests
    {
        [Fact]
        public void ApplicationUser_ShouldInheritFromIdentityUser()
        {
            // Arrange
            var user = new ApplicationUser();

            // Assert
            Assert.IsAssignableFrom<Microsoft.AspNetCore.Identity.IdentityUser>(user);
        }

        [Fact]
        public void ApplicationUser_ShouldHaveDisplayNameProperty()
        {
            // Arrange
            var user = new ApplicationUser();
            var testDisplayName = "Test User";

            // Act
            user.DisplayName = testDisplayName;

            // Assert
            Assert.Equal(testDisplayName, user.DisplayName);
        }

        [Fact]
        public void ApplicationUser_ShouldOverridePhoneNumberProperty()
        {
            // Arrange
            var user = new ApplicationUser();
            var testPhone = "+40123456789";

            // Act
            user.PhoneNumber = testPhone;

            // Assert
            Assert.Equal(testPhone, user.PhoneNumber);
        }
    }
}
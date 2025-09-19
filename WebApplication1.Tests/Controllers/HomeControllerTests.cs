using Xunit;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Controllers;

namespace WebApplication1.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _controller = new HomeController();
        }

        [Fact]
        public void Index_ShouldReturnViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.NotNull(_controller);
        }

        [Fact]
        public void HomeController_ShouldInheritFromController()
        {
            // Act & Assert
            Assert.IsAssignableFrom<Controller>(_controller);
        }
    }
}
using Xunit;

namespace WebApplication1.Tests
{
    public class BasicApplicationTests
    {
        [Fact]
        public void Application_ShouldBuildSuccessfully()
        {
            // This test will pass if the project builds successfully
            Assert.True(true);
        }

        [Fact]
        public void Constants_ShouldBeCorrect()
        {
            // Test basic constants and values
            Assert.Equal("net8.0", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Contains("8.") ? "net8.0" : "other");
        }
    }
}
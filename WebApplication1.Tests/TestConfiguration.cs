using Xunit;

// Global test configuration
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 4)]

// Test configuration attributes for different categories
namespace WebApplication1.Tests
{
    /// <summary>
    /// Marks tests that require database access
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class DatabaseTestAttribute : Attribute
    {
        public DatabaseTestAttribute() { }
    }

    /// <summary>
    /// Marks integration tests
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class IntegrationTestAttribute : Attribute
    {
        public IntegrationTestAttribute() { }
    }

    /// <summary>
    /// Marks performance tests
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class PerformanceTestAttribute : Attribute
    {
        public PerformanceTestAttribute() { }
    }

    /// <summary>
    /// Marks unit tests
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class UnitTestAttribute : Attribute
    {
        public UnitTestAttribute() { }
    }

    /// <summary>
    /// Marks tests that are slow to execute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SlowTestAttribute : Attribute
    {
        public SlowTestAttribute() { }
    }

    /// <summary>
    /// Marks tests that require external services
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class ExternalServiceTestAttribute : Attribute
    {
        public ExternalServiceTestAttribute() { }
    }
}
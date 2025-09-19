using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using WebApplication1.Tests.Integration;

namespace WebApplication1.Tests.Performance
{
    public class PerformanceTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PerformanceTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HomePage_ShouldLoadWithinAcceptableTime()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/");

            // Assert
            stopwatch.Stop();
            response.EnsureSuccessStatusCode();
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Home page took {stopwatch.ElapsedMilliseconds}ms to load");
        }

        [Fact]
        public async Task LoginPage_ShouldLoadWithinAcceptableTime()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/Account/Login");

            // Assert
            stopwatch.Stop();
            response.EnsureSuccessStatusCode();
            Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Login page took {stopwatch.ElapsedMilliseconds}ms to load");
        }

        [Fact]
        public async Task Database_ShouldHandleMultipleSimultaneousReads()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Add test data first
            var appointments = new List<Appointment>();
            for (int i = 0; i < 100; i++)
            {
                appointments.Add(new Appointment
                {
                    Title = $"Test Appointment {i}",
                    ClientName = $"Client {i}",
                    AppointmentDate = DateTime.Now.AddHours(i),
                    DurationMinutes = 60,
                    CreatedAt = DateTime.Now
                });
            }
            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();

            var stopwatch = Stopwatch.StartNew();

            // Act - Simulate multiple simultaneous reads
            var tasks = new List<Task<List<Appointment>>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    using var taskScope = _factory.Services.CreateScope();
                    var taskContext = taskScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    return await taskContext.Appointments.Take(10).ToListAsync();
                }));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Multiple reads took {stopwatch.ElapsedMilliseconds}ms");
            Assert.All(results, result => Assert.True(result.Count <= 10, "Each query should return at most 10 results"));
        }

        [Fact]
        public async Task Database_ShouldHandleLargeDatasets()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var appointments = new List<Appointment>();
            for (int i = 0; i < 1000; i++)
            {
                appointments.Add(new Appointment
                {
                    Title = $"Bulk Test Appointment {i}",
                    ClientName = $"Bulk Client {i}",
                    AppointmentDate = DateTime.Now.AddHours(i % 24),
                    DurationMinutes = 60,
                    Status = (AppointmentStatus)(i % Enum.GetValues<AppointmentStatus>().Length)
                });
            }

            var stopwatch = Stopwatch.StartNew();

            // Act
            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, $"Bulk insert took {stopwatch.ElapsedMilliseconds}ms");

            // Verify data was inserted
            var count = await context.Appointments.CountAsync();
            Assert.True(count >= 1000, $"Expected at least 1000 appointments, found {count}");
        }

        [Fact]
        public async Task Database_QueryPerformance_ShouldBeAcceptable()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Add test data if needed
            if (!await context.Appointments.AnyAsync())
            {
                for (int i = 0; i < 500; i++)
                {
                    context.Appointments.Add(new Appointment
                    {
                        Title = $"Query Test Appointment {i}",
                        ClientName = $"Query Client {i}",
                        AppointmentDate = DateTime.Now.AddDays(i % 30),
                        DurationMinutes = 60,
                        Status = (AppointmentStatus)(i % Enum.GetValues<AppointmentStatus>().Length)
                    });
                }
                await context.SaveChangesAsync();
            }

            var stopwatch = Stopwatch.StartNew();

            // Act - Complex query
            var results = await context.Appointments
                .Where(a => a.AppointmentDate >= DateTime.Today)
                .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.AppointmentDate)
                .Take(50)
                .ToListAsync();

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 2000, $"Complex query took {stopwatch.ElapsedMilliseconds}ms");
            Assert.True(results.Count > 0, "Query should return results");
        }

        [Fact]
        public async Task MemoryUsage_ShouldBeReasonable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(false);

            // Act - Perform memory-intensive operations
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var appointments = new List<Appointment>();
            for (int i = 0; i < 1000; i++)
            {
                appointments.Add(new Appointment
                {
                    Title = $"Memory Test {i}",
                    ClientName = $"Client {i}",
                    AppointmentDate = DateTime.Now.AddHours(i),
                    DurationMinutes = 60
                });
            }

            context.Appointments.AddRange(appointments);
            await context.SaveChangesAsync();

            var afterAddMemory = GC.GetTotalMemory(false);
            
            // Query the data
            var results = await context.Appointments
                .Where(a => a.Title.Contains("Memory Test"))
                .ToListAsync();

            var afterQueryMemory = GC.GetTotalMemory(false);

            // Assert
            var memoryIncrease = afterQueryMemory - initialMemory;
            Assert.True(memoryIncrease < 50 * 1024 * 1024, $"Memory usage increased by {memoryIncrease / 1024 / 1024}MB");
            Assert.Equal(1000, results.Count);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public async Task ConcurrentUserSimulation_ShouldHandleLoad(int userCount)
        {
            // Arrange
            var tasks = new List<Task<bool>>();
            var stopwatch = Stopwatch.StartNew();

            // Act - Simulate concurrent users
            for (int i = 0; i < userCount; i++)
            {
                tasks.Add(SimulateUserSession(i));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < userCount * 100, $"Concurrent users test took {stopwatch.ElapsedMilliseconds}ms for {userCount} users");
            Assert.All(results, result => Assert.True(result, "All user sessions should complete successfully"));
        }

        private async Task<bool> SimulateUserSession(int userId)
        {
            try
            {
                // Simulate user browsing
                var homeResponse = await _client.GetAsync("/");
                homeResponse.EnsureSuccessStatusCode();

                var loginResponse = await _client.GetAsync("/Account/Login");
                loginResponse.EnsureSuccessStatusCode();

                var registerResponse = await _client.GetAsync("/Account/Register");
                registerResponse.EnsureSuccessStatusCode();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
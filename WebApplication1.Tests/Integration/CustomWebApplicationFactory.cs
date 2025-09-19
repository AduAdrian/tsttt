using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Tests.Integration
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Initialize test data
                InitializeTestDataAsync(db, userManager, roleManager).GetAwaiter().GetResult();
            });

            builder.UseEnvironment("Testing");
        }

        private async Task InitializeTestDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Create Admin role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Create test admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "testadmin",
                    Email = "admin@test.com",
                    EmailConfirmed = true,
                    DisplayName = "Test Administrator",
                    PhoneNumber = "+40123456789"
                };

                if (await userManager.FindByNameAsync(adminUser.UserName) == null)
                {
                    var result = await userManager.CreateAsync(adminUser, "TestPass123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Create test regular user
                var testUser = new ApplicationUser
                {
                    UserName = "testuser",
                    Email = "user@test.com",
                    EmailConfirmed = true,
                    DisplayName = "Test User",
                    PhoneNumber = "+40987654321"
                };

                if (await userManager.FindByNameAsync(testUser.UserName) == null)
                {
                    await userManager.CreateAsync(testUser, "TestPass123!");
                }

                // Add some test appointments
                if (!context.Appointments.Any())
                {
                    var appointments = new List<Appointment>
                    {
                        new Appointment
                        {
                            Title = "Test Appointment 1",
                            Description = "Test Description 1",
                            ClientName = "Test Client 1",
                            ClientEmail = "client1@test.com",
                            ClientPhone = "+40111111111",
                            AppointmentDate = DateTime.Now.AddDays(1),
                            DurationMinutes = 60,
                            Status = AppointmentStatus.Scheduled,
                            Location = "Test Location 1",
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.Now
                        },
                        new Appointment
                        {
                            Title = "Test Appointment 2",
                            Description = "Test Description 2",
                            ClientName = "Test Client 2",
                            ClientEmail = "client2@test.com",
                            ClientPhone = "+40222222222",
                            AppointmentDate = DateTime.Now.AddDays(2),
                            DurationMinutes = 90,
                            Status = AppointmentStatus.Confirmed,
                            Location = "Test Location 2",
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.Now
                        }
                    };

                    context.Appointments.AddRange(appointments);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception in a real scenario
                System.Diagnostics.Debug.WriteLine($"Error initializing test data: {ex.Message}");
            }
        }
    }
}
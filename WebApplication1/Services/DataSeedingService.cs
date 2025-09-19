using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Services
{
    public interface IDataSeedingService
    {
        Task SeedClientsAsync();
        Task<bool> HasSeedDataAsync();
    }

    public class DataSeedingService : IDataSeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataSeedingService> _logger;

        public DataSeedingService(ApplicationDbContext context, ILogger<DataSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasSeedDataAsync()
        {
            return await _context.Clients.AnyAsync();
        }

        public async Task SeedClientsAsync()
        {
            if (await HasSeedDataAsync())
            {
                _logger.LogInformation("Seed data already exists. Skipping seeding.");
                return;
            }

            _logger.LogInformation("Starting to seed 100 client records...");

            var random = new Random();
            var clients = new List<Client>();

            // Romanian county codes for realistic registration numbers
            var countyCodes = new[] { "B", "AB", "AG", "AR", "BC", "BH", "BN", "BR", "BT", "BV", "BZ", "CJ", "CL", "CS", "CT", "CV", "DB", "DJ", "GJ", "GL", "GR", "HD", "HR", "IF", "IL", "IS", "MH", "MM", "MS", "NT", "OT", "PH", "SB", "SJ", "SM", "SV", "TL", "TM", "TR", "VL", "VN", "VS" };
            
            // Car brands for more realistic registration numbers
            var letterCombinations = new[] { "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX", "YZA", "BCD", "EFG", "HIJ", "KLM", "NOP", "QRS", "TUV", "WXY", "ZAB" };

            // Validity types with weights (more realistic distribution)
            var validityTypes = new[]
            {
                (ValidityType.Manual, 20),      // 20% manual
                (ValidityType.SixMonths, 30),   // 30% 6 months
                (ValidityType.OneYear, 40),     // 40% 1 year
                (ValidityType.TwoYears, 10)     // 10% 2 years
            };

            var today = DateTime.Today;

            for (int i = 1; i <= 100; i++)
            {
                // Generate registration number
                var countyCode = countyCodes[random.Next(countyCodes.Length)];
                var numbers = random.Next(10, 999).ToString("D2");
                var letters = letterCombinations[random.Next(letterCombinations.Length)];
                var registrationNumber = $"{countyCode}{numbers}{letters}";

                // Select validity type based on weights
                var totalWeight = validityTypes.Sum(v => v.Item2);
                var randomWeight = random.Next(totalWeight);
                var cumulativeWeight = 0;
                ValidityType selectedValidityType = ValidityType.Manual;

                foreach (var (type, weight) in validityTypes)
                {
                    cumulativeWeight += weight;
                    if (randomWeight < cumulativeWeight)
                    {
                        selectedValidityType = type;
                        break;
                    }
                }

                // Calculate expiry date
                DateTime expiryDate;
                if (selectedValidityType == ValidityType.Manual)
                {
                    // Random date within next 2 years for manual entries
                    var maxDaysInFuture = 730; // 2 years
                    var randomDays = random.Next(-30, maxDaysInFuture); // Some can be expired (past 30 days)
                    expiryDate = today.AddDays(randomDays);
                }
                else
                {
                    // Calculate based on validity type from a random past date
                    var monthsAgo = random.Next(0, (int)selectedValidityType); // Started somewhere in the validity period
                    var startDate = today.AddMonths(-monthsAgo);
                    expiryDate = startDate.AddMonths((int)selectedValidityType);
                }

                // Create client
                var client = new Client
                {
                    RegistrationNumber = registrationNumber,
                    ValidityType = selectedValidityType,
                    ExpiryDate = expiryDate,
                    PhoneNumber = "0756596565", // All clients have the same phone number as requested
                    CreatedAt = today.AddDays(-random.Next(0, 365)), // Created within last year
                    IsActive = true
                };

                clients.Add(client);
            }

            // Add all clients to database
            await _context.Clients.AddRangeAsync(clients);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully seeded {clients.Count} client records.");

            // Log some statistics
            var expiredCount = clients.Count(c => c.ExpiryDate < today);
            var expiringSoonCount = clients.Count(c => c.ExpiryDate >= today && (c.ExpiryDate - today).TotalDays <= 30);
            var validCount = clients.Count(c => (c.ExpiryDate - today).TotalDays > 30);

            _logger.LogInformation($"Seed data statistics: {expiredCount} expired, {expiringSoonCount} expiring soon, {validCount} valid");
        }
    }
}
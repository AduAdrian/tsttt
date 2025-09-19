using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Services
{
    public interface IItpNotificationService
    {
        Task<int> SendExpiringNotificationsAsync();
        Task<int> SendNotificationsForDaysAheadAsync(int daysAhead);
        Task<bool> SendNotificationToClientAsync(int clientId, NotificationType type = NotificationType.ItpExpiring, int daysAhead = 7);
        Task<List<Client>> GetClientsEligibleForNotificationAsync(int daysAhead = 7);
        Task<bool> HasRecentNotificationAsync(int clientId, DateTime expiryDate, NotificationType type);
        string CreateNotificationMessage(string registrationNumber, int daysUntilExpiry);
        Task<List<SmsNotification>> GetNotificationHistoryAsync(int? clientId = null, int pageSize = 50);
        Task<NotificationSummary> GetNotificationSummaryAsync();
    }

    public class ItpNotificationService : IItpNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISmsService _smsService;
        private readonly ILogger<ItpNotificationService> _logger;

        private readonly string _notificationMessage = "Bun? ziua, expir? ITP la auto cu {0}. Pentru o nou? inspec?ie tehnic?, v? rug?m apela?i 0756596565-Adrian, 0745025533-Vasile sau v? a?tept?m la sta?ia ITP pe Izvoarelor 2C bis R?d?u?i. Zi bun? v? dorim!";

        public ItpNotificationService(
            ApplicationDbContext context,
            ISmsService smsService,
            ILogger<ItpNotificationService> logger)
        {
            _context = context;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<int> SendExpiringNotificationsAsync()
        {
            var currentHour = DateTime.Now.Hour;

            // Verific? dac? suntem în intervalul de lucru (8:00-21:00)
            if (currentHour < 8 || currentHour >= 21)
            {
                _logger.LogInformation("În afara orelor de lucru. Notific?rile vor fi programate pentru mai târziu.");
                await ScheduleNotificationsAsync();
                return 0;
            }

            var totalSent = 0;

            try
            {
                // Trimite notific?ri pentru 30 zile
                var sent30Days = await SendNotificationsForDaysAheadAsync(30);
                totalSent += sent30Days;

                // Pauz? între diferitele tipuri de notific?ri
                await Task.Delay(1000);

                // Trimite notific?ri pentru 7 zile
                var sent7Days = await SendNotificationsForDaysAheadAsync(7);
                totalSent += sent7Days;

                // Pauz? între diferitele tipuri de notific?ri
                await Task.Delay(1000);

                // Trimite notific?ri pentru 1 zi
                var sent1Day = await SendNotificationsForDaysAheadAsync(1);
                totalSent += sent1Day;

                _logger.LogInformation($"Total notific?ri trimise: {totalSent} (30 zile: {sent30Days}, 7 zile: {sent7Days}, 1 zi: {sent1Day})");
                return totalSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare în SendExpiringNotificationsAsync");
                return totalSent;
            }
        }

        public async Task<int> SendNotificationsForDaysAheadAsync(int daysAhead)
        {
            var sentCount = 0;
            var currentHour = DateTime.Now.Hour;

            // Verific? din nou orele de lucru
            if (currentHour < 8 || currentHour >= 21)
            {
                _logger.LogInformation($"În afara orelor de lucru pentru notific?ri {daysAhead} zile.");
                return 0;
            }

            try
            {
                var notificationType = GetNotificationTypeByDays(daysAhead);
                var targetDate = DateTime.Today.AddDays(daysAhead);
                
                var eligibleClients = await _context.Clients
                    .Where(c => c.IsActive 
                            && !string.IsNullOrEmpty(c.PhoneNumber)
                            && c.ExpiryDate.Date == targetDate)
                    .OrderBy(c => c.ExpiryDate)
                    .ToListAsync();

                _logger.LogInformation($"G?si?i {eligibleClients.Count} clien?i eligibili pentru notific?ri ITP la {daysAhead} zile");

                foreach (var client in eligibleClients)
                {
                    try
                    {
                        // Verific? dac? nu a fost deja trimis? notificarea pentru aceast? dat? ?i tip
                        var hasRecentNotification = await HasRecentNotificationForDaysAsync(
                            client.Id, 
                            client.ExpiryDate, 
                            daysAhead);

                        if (hasRecentNotification)
                        {
                            _logger.LogInformation($"Clientul {client.RegistrationNumber} are deja notificare recent? pentru {daysAhead} zile");
                            continue;
                        }

                        var success = await SendNotificationToClientAsync(client.Id, notificationType, daysAhead);
                        if (success)
                        {
                            sentCount++;
                            _logger.LogInformation($"Trimis cu succes notificare {daysAhead} zile c?tre {client.RegistrationNumber} ({client.PhoneNumber})");
                        }
                        else
                        {
                            _logger.LogWarning($"E?uat trimiterea notific?rii c?tre {client.RegistrationNumber}");
                        }

                        // Pauz? între mesaje pentru a evita spam-ul
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Eroare la trimiterea notific?rii c?tre clientul {client.RegistrationNumber}");
                    }
                }

                _logger.LogInformation($"Finalizat trimiterea a {sentCount} notific?ri pentru {daysAhead} zile");
                return sentCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eroare în SendNotificationsForDaysAheadAsync pentru {daysAhead} zile");
                return 0;
            }
        }

        public async Task<bool> SendNotificationToClientAsync(int clientId, NotificationType type = NotificationType.ItpExpiring, int daysAhead = 7)
        {
            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.Id == clientId && c.IsActive);

                if (client == null)
                {
                    _logger.LogWarning($"Clientul cu ID {clientId} nu a fost g?sit sau este inactiv");
                    return false;
                }

                if (string.IsNullOrEmpty(client.PhoneNumber))
                {
                    _logger.LogWarning($"Clientul {client.RegistrationNumber} nu are num?r de telefon");
                    return false;
                }

                var daysUntilExpiry = (client.ExpiryDate - DateTime.Today).Days;
                var message = CreateNotificationMessage(client.RegistrationNumber, daysUntilExpiry);

                // Creeaz? înregistrarea în baza de date
                var notification = new SmsNotification
                {
                    ClientId = clientId,
                    PhoneNumber = client.PhoneNumber,
                    Message = message,
                    Type = type,
                    Status = NotificationStatus.Pending,
                    ExpiryDateSnapshot = client.ExpiryDate,
                    NotificationDays = daysAhead,
                    CreatedAt = DateTime.Now
                };

                _context.SmsNotifications.Add(notification);
                await _context.SaveChangesAsync();

                // Încearc? s? trimit? SMS-ul
                try
                {
                    var smsResult = await _smsService.SendSmsAsync(client.PhoneNumber, message);

                    // Actualizeaz? statusul notific?rii
                    if (smsResult)
                    {
                        notification.Status = NotificationStatus.Sent;
                        notification.SentAt = DateTime.Now;
                        _logger.LogInformation($"SMS trimis cu succes c?tre {client.PhoneNumber} pentru {client.RegistrationNumber}");
                    }
                    else
                    {
                        notification.Status = NotificationStatus.Failed;
                        notification.ErrorMessage = "Serviciul SMS a returnat false";
                        _logger.LogError($"Serviciul SMS a e?uat pentru {client.PhoneNumber}");
                    }
                }
                catch (Exception ex)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage = ex.Message;
                    _logger.LogError(ex, $"Excep?ie la trimiterea SMS c?tre {client.PhoneNumber}");
                }

                await _context.SaveChangesAsync();
                return notification.Status == NotificationStatus.Sent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eroare în SendNotificationToClientAsync pentru clientul {clientId}");
                return false;
            }
        }

        public async Task<List<Client>> GetClientsEligibleForNotificationAsync(int daysAhead = 7)
        {
            var targetDate = DateTime.Today.AddDays(daysAhead);

            return await _context.Clients
                .Where(c => c.IsActive 
                        && !string.IsNullOrEmpty(c.PhoneNumber)
                        && c.ExpiryDate.Date == targetDate)
                .OrderBy(c => c.ExpiryDate)
                .ToListAsync();
        }

        public async Task<bool> HasRecentNotificationAsync(int clientId, DateTime expiryDate, NotificationType type)
        {
            // Verific? dac? exist? o notificare cu succes pentru aceast? dat? de expirare în ultimele 24 ore
            var oneDayAgo = DateTime.Now.AddDays(-1);

            return await _context.SmsNotifications
                .AnyAsync(n => n.ClientId == clientId 
                            && n.Type == type
                            && n.ExpiryDateSnapshot.Date == expiryDate.Date
                            && n.Status == NotificationStatus.Sent
                            && n.CreatedAt >= oneDayAgo);
        }

        private async Task<bool> HasRecentNotificationForDaysAsync(int clientId, DateTime expiryDate, int daysAhead)
        {
            // Verific? dac? exist? o notificare cu succes pentru acest tip în ultimele 24 ore
            var oneDayAgo = DateTime.Now.AddDays(-1);

            return await _context.SmsNotifications
                .AnyAsync(n => n.ClientId == clientId 
                            && n.NotificationDays == daysAhead
                            && n.ExpiryDateSnapshot.Date == expiryDate.Date
                            && n.Status == NotificationStatus.Sent
                            && n.CreatedAt >= oneDayAgo);
        }

        public string CreateNotificationMessage(string registrationNumber, int daysUntilExpiry)
        {
            var dayText = daysUntilExpiry switch
            {
                0 => "ast?zi",
                1 => "mâine", 
                _ => $"în {daysUntilExpiry} zile"
            };

            return string.Format(_notificationMessage, $"{registrationNumber} {dayText}");
        }

        public async Task<List<SmsNotification>> GetNotificationHistoryAsync(int? clientId = null, int pageSize = 50)
        {
            var query = _context.SmsNotifications
                .Include(n => n.Client)
                .AsQueryable();

            if (clientId.HasValue)
            {
                query = query.Where(n => n.ClientId == clientId.Value);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<NotificationSummary> GetNotificationSummaryAsync()
        {
            var today = DateTime.Today;
            
            return new NotificationSummary
            {
                Clients30Days = await GetClientsCountByDaysAsync(30),
                Clients7Days = await GetClientsCountByDaysAsync(7),
                Clients1Day = await GetClientsCountByDaysAsync(1),
                
                Sent30Days = await GetSentNotificationsCountByDaysAsync(30),
                Sent7Days = await GetSentNotificationsCountByDaysAsync(7),
                Sent1Day = await GetSentNotificationsCountByDaysAsync(1),
                
                NotSent30Days = await GetNotSentNotificationsCountByDaysAsync(30),
                NotSent7Days = await GetNotSentNotificationsCountByDaysAsync(7),
                NotSent1Day = await GetNotSentNotificationsCountByDaysAsync(1)
            };
        }

        private async Task<int> GetClientsCountByDaysAsync(int daysAhead)
        {
            var targetDate = DateTime.Today.AddDays(daysAhead);
            return await _context.Clients
                .CountAsync(c => c.IsActive && c.ExpiryDate.Date == targetDate);
        }

        private async Task<int> GetSentNotificationsCountByDaysAsync(int daysAhead)
        {
            var targetDate = DateTime.Today.AddDays(daysAhead);
            return await _context.SmsNotifications
                .CountAsync(n => n.NotificationDays == daysAhead 
                            && n.Status == NotificationStatus.Sent
                            && n.ExpiryDateSnapshot.Date == targetDate);
        }

        private async Task<int> GetNotSentNotificationsCountByDaysAsync(int daysAhead)
        {
            var targetDate = DateTime.Today.AddDays(daysAhead);
            var clientsCount = await GetClientsCountByDaysAsync(daysAhead);
            var sentCount = await GetSentNotificationsCountByDaysAsync(daysAhead);
            return Math.Max(0, clientsCount - sentCount);
        }

        private NotificationType GetNotificationTypeByDays(int daysAhead)
        {
            return daysAhead switch
            {
                30 => NotificationType.ItpExpiring30Days,
                7 => NotificationType.ItpExpiring7Days,
                1 => NotificationType.ItpExpiring1Day,
                0 => NotificationType.ItpExpired,
                _ => NotificationType.ItpExpiring
            };
        }

        private async Task ScheduleNotificationsAsync()
        {
            var tomorrowMorning = DateTime.Today.AddDays(1).AddHours(8);

            // Programeaz? pentru fiecare tip de notificare
            foreach (var daysAhead in new[] { 30, 7, 1 })
            {
                var eligibleClients = await GetClientsEligibleForNotificationAsync(daysAhead);
                var notificationType = GetNotificationTypeByDays(daysAhead);

                foreach (var client in eligibleClients)
                {
                    var hasScheduled = await _context.SmsNotifications
                        .AnyAsync(n => n.ClientId == client.Id 
                                    && n.NotificationDays == daysAhead
                                    && n.Status == NotificationStatus.Scheduled
                                    && n.ScheduledFor.HasValue 
                                    && n.ScheduledFor.Value.Date == tomorrowMorning.Date);

                    if (!hasScheduled)
                    {
                        var message = CreateNotificationMessage(client.RegistrationNumber, (client.ExpiryDate - DateTime.Today).Days);

                        var scheduledNotification = new SmsNotification
                        {
                            ClientId = client.Id,
                            PhoneNumber = client.PhoneNumber!,
                            Message = message,
                            Type = notificationType,
                            Status = NotificationStatus.Scheduled,
                            ScheduledFor = tomorrowMorning,
                            ExpiryDateSnapshot = client.ExpiryDate,
                            NotificationDays = daysAhead
                        };

                        _context.SmsNotifications.Add(scheduledNotification);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    // Model pentru rezumatul notific?rilor
    public class NotificationSummary
    {
        public int Clients30Days { get; set; }
        public int Clients7Days { get; set; }
        public int Clients1Day { get; set; }
        
        public int Sent30Days { get; set; }
        public int Sent7Days { get; set; }
        public int Sent1Day { get; set; }
        
        public int NotSent30Days { get; set; }
        public int NotSent7Days { get; set; }
        public int NotSent1Day { get; set; }
    }
}
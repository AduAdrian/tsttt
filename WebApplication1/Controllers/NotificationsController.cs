using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IItpNotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            ApplicationDbContext context,
            IItpNotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: Notifications/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var today = DateTime.Today;

                // Ob?ine rezumatul notific?rilor
                var summary = await _notificationService.GetNotificationSummaryAsync();

                var viewModel = new NotificationDashboardViewModel
                {
                    TotalNotifications = await _context.SmsNotifications.CountAsync(),
                    SentToday = await _context.SmsNotifications
                        .CountAsync(n => n.SentAt.HasValue && n.SentAt.Value.Date == today),
                    PendingNotifications = await _context.SmsNotifications
                        .CountAsync(n => n.Status == NotificationStatus.Pending),
                    FailedNotifications = await _context.SmsNotifications
                        .CountAsync(n => n.Status == NotificationStatus.Failed),
                    ClientsWithoutPhone = await _context.Clients
                        .CountAsync(c => c.IsActive && string.IsNullOrEmpty(c.PhoneNumber)),
                    
                    // Statistici pe zile
                    Clients30Days = summary.Clients30Days,
                    Clients7Days = summary.Clients7Days,
                    Clients1Day = summary.Clients1Day,
                    
                    Sent30Days = summary.Sent30Days,
                    Sent7Days = summary.Sent7Days,
                    Sent1Day = summary.Sent1Day,
                    
                    NotSent30Days = summary.NotSent30Days,
                    NotSent7Days = summary.NotSent7Days,
                    NotSent1Day = summary.NotSent1Day
                };

                // Notific?ri recente
                viewModel.RecentNotifications = await _context.SmsNotifications
                    .Include(n => n.Client)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(15)
                    .Select(n => new NotificationListViewModel
                    {
                        Id = n.Id,
                        ClientRegistrationNumber = n.Client.RegistrationNumber,
                        PhoneNumber = n.PhoneNumber,
                        Status = n.Status,
                        Type = n.Type,
                        NotificationDays = n.NotificationDays,
                        CreatedAt = n.CreatedAt,
                        SentAt = n.SentAt,
                        ErrorMessage = n.ErrorMessage,
                        DaysUntilExpiry = (n.Client.ExpiryDate - today).Days
                    })
                    .ToListAsync();

                // Expir?ri pe 30 de zile
                var date30Days = today.AddDays(30);
                viewModel.UpcomingExpirations30Days = await _context.Clients
                    .Where(c => c.IsActive && c.ExpiryDate.Date == date30Days)
                    .OrderBy(c => c.ExpiryDate)
                    .Take(10)
                    .Select(c => new ClientListViewModel
                    {
                        Id = c.Id,
                        RegistrationNumber = c.RegistrationNumber,
                        ExpiryDate = c.ExpiryDate,
                        PhoneNumber = c.PhoneNumber,
                        ValidityType = c.ValidityType,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                // Expir?ri pe 7 zile
                var date7Days = today.AddDays(7);
                viewModel.UpcomingExpirations7Days = await _context.Clients
                    .Where(c => c.IsActive && c.ExpiryDate.Date == date7Days)
                    .OrderBy(c => c.ExpiryDate)
                    .Take(10)
                    .Select(c => new ClientListViewModel
                    {
                        Id = c.Id,
                        RegistrationNumber = c.RegistrationNumber,
                        ExpiryDate = c.ExpiryDate,
                        PhoneNumber = c.PhoneNumber,
                        ValidityType = c.ValidityType,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                // Expir?ri pe 1 zi
                var date1Day = today.AddDays(1);
                viewModel.UpcomingExpirations1Day = await _context.Clients
                    .Where(c => c.IsActive && c.ExpiryDate.Date == date1Day)
                    .OrderBy(c => c.ExpiryDate)
                    .Take(10)
                    .Select(c => new ClientListViewModel
                    {
                        Id = c.Id,
                        RegistrationNumber = c.RegistrationNumber,
                        ExpiryDate = c.ExpiryDate,
                        PhoneNumber = c.PhoneNumber,
                        ValidityType = c.ValidityType,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                // Clien?i care au nevoie de num?r de telefon
                viewModel.ClientsNeedingPhone = await _context.Clients
                    .Where(c => c.IsActive && string.IsNullOrEmpty(c.PhoneNumber))
                    .OrderBy(c => c.ExpiryDate)
                    .Take(10)
                    .Select(c => new ClientListViewModel
                    {
                        Id = c.Id,
                        RegistrationNumber = c.RegistrationNumber,
                        ExpiryDate = c.ExpiryDate,
                        PhoneNumber = c.PhoneNumber,
                        ValidityType = c.ValidityType,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la înc?rcarea dashboard-ului de notific?ri");
                TempData["ErrorMessage"] = "Eroare la înc?rcarea dashboard-ului de notific?ri.";
                return View(new NotificationDashboardViewModel());
            }
        }

        // GET: Notifications
        public async Task<IActionResult> Index(NotificationHistoryViewModel filter)
        {
            try
            {
                var query = _context.SmsNotifications
                    .Include(n => n.Client)
                    .AsQueryable();

                // Aplic? filtrele
                if (!string.IsNullOrEmpty(filter.FilterStatus) && 
                    Enum.TryParse<NotificationStatus>(filter.FilterStatus, out var status))
                {
                    query = query.Where(n => n.Status == status);
                }

                if (!string.IsNullOrEmpty(filter.FilterType) && 
                    Enum.TryParse<NotificationType>(filter.FilterType, out var type))
                {
                    query = query.Where(n => n.Type == type);
                }

                if (!string.IsNullOrEmpty(filter.FilterDays) && int.TryParse(filter.FilterDays, out var days))
                {
                    query = query.Where(n => n.NotificationDays == days);
                }

                if (filter.FilterDateFrom.HasValue)
                {
                    query = query.Where(n => n.CreatedAt >= filter.FilterDateFrom.Value);
                }

                if (filter.FilterDateTo.HasValue)
                {
                    var dateTo = filter.FilterDateTo.Value.Date.AddDays(1);
                    query = query.Where(n => n.CreatedAt < dateTo);
                }

                if (!string.IsNullOrEmpty(filter.FilterClientRegistration))
                {
                    query = query.Where(n => n.Client.RegistrationNumber.Contains(filter.FilterClientRegistration));
                }

                var totalCount = await query.CountAsync();
                
                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(n => new NotificationListViewModel
                    {
                        Id = n.Id,
                        ClientRegistrationNumber = n.Client.RegistrationNumber,
                        PhoneNumber = n.PhoneNumber,
                        Message = n.Message.Length > 100 ? n.Message.Substring(0, 100) + "..." : n.Message,
                        Status = n.Status,
                        Type = n.Type,
                        NotificationDays = n.NotificationDays,
                        CreatedAt = n.CreatedAt,
                        SentAt = n.SentAt,
                        ErrorMessage = n.ErrorMessage,
                        DaysUntilExpiry = (n.Client.ExpiryDate - DateTime.Today).Days
                    })
                    .ToListAsync();

                var viewModel = new NotificationHistoryViewModel
                {
                    Notifications = notifications,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    FilterStatus = filter.FilterStatus,
                    FilterType = filter.FilterType,
                    FilterDays = filter.FilterDays,
                    FilterDateFrom = filter.FilterDateFrom,
                    FilterDateTo = filter.FilterDateTo,
                    FilterClientRegistration = filter.FilterClientRegistration
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la înc?rcarea istoricului notific?rilor");
                TempData["ErrorMessage"] = "Eroare la înc?rcarea istoricului notific?rilor.";
                return View(new NotificationHistoryViewModel());
            }
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var notification = await _context.SmsNotifications
                .Include(n => n.Client)
                .Include(n => n.CreatedByUser)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/SendToClient
        [HttpPost]
        public async Task<IActionResult> SendToClient(int clientId, int daysAhead = 7)
        {
            try
            {
                var notificationType = daysAhead switch
                {
                    30 => NotificationType.ItpExpiring30Days,
                    7 => NotificationType.ItpExpiring7Days,
                    1 => NotificationType.ItpExpiring1Day,
                    _ => NotificationType.ItpExpiring7Days
                };

                var success = await _notificationService.SendNotificationToClientAsync(clientId, notificationType, daysAhead);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Notificarea a fost trimis? cu succes!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Eroare la trimiterea notific?rii.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eroare la trimiterea notific?rii c?tre clientul {clientId}");
                TempData["ErrorMessage"] = "Eroare la trimiterea notific?rii.";
            }

            return RedirectToAction("Dashboard");
        }

        // POST: Notifications/RunNotifications
        [HttpPost]
        public async Task<IActionResult> RunNotifications()
        {
            try
            {
                var sentCount = await _notificationService.SendExpiringNotificationsAsync();
                TempData["SuccessMessage"] = $"Au fost trimise {sentCount} notific?ri pentru ITP-uri care expir?.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la rularea manual? a notific?rilor");
                TempData["ErrorMessage"] = "Eroare la rularea notific?rilor.";
            }

            return RedirectToAction("Dashboard");
        }

        // POST: Notifications/RunNotifications30Days
        [HttpPost]
        public async Task<IActionResult> RunNotifications30Days()
        {
            try
            {
                var sentCount = await _notificationService.SendNotificationsForDaysAheadAsync(30);
                TempData["SuccessMessage"] = $"Au fost trimise {sentCount} notific?ri pentru ITP-uri care expir? în 30 de zile.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la rularea notific?rilor pentru 30 de zile");
                TempData["ErrorMessage"] = "Eroare la rularea notific?rilor pentru 30 de zile.";
            }

            return RedirectToAction("Dashboard");
        }

        // POST: Notifications/RunNotifications7Days
        [HttpPost]
        public async Task<IActionResult> RunNotifications7Days()
        {
            try
            {
                var sentCount = await _notificationService.SendNotificationsForDaysAheadAsync(7);
                TempData["SuccessMessage"] = $"Au fost trimise {sentCount} notific?ri pentru ITP-uri care expir? în 7 zile.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la rularea notific?rilor pentru 7 zile");
                TempData["ErrorMessage"] = "Eroare la rularea notific?rilor pentru 7 zile.";
            }

            return RedirectToAction("Dashboard");
        }

        // POST: Notifications/RunNotifications1Day
        [HttpPost]
        public async Task<IActionResult> RunNotifications1Day()
        {
            try
            {
                var sentCount = await _notificationService.SendNotificationsForDaysAheadAsync(1);
                TempData["SuccessMessage"] = $"Au fost trimise {sentCount} notific?ri pentru ITP-uri care expir? în 1 zi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la rularea notific?rilor pentru 1 zi");
                TempData["ErrorMessage"] = "Eroare la rularea notific?rilor pentru 1 zi.";
            }

            return RedirectToAction("Dashboard");
        }
    }
}
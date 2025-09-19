using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AppointmentsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(DateTime? filterDate, AppointmentStatus? filterStatus, string? filterClient)
        {
            var appointmentsQuery = _context.Appointments
                .Include(a => a.CreatedByUser)
                .AsQueryable();

            // Apply filters
            if (filterDate.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate.Date == filterDate.Value.Date);
            }

            if (filterStatus.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.Status == filterStatus.Value);
            }

            if (!string.IsNullOrEmpty(filterClient))
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.ClientName.Contains(filterClient));
            }

            var appointments = await appointmentsQuery
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // Get statistics
            var totalAppointments = await _context.Appointments.CountAsync();
            var todayAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Date == DateTime.Today);
            var upcomingAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate > DateTime.Now && a.Status != AppointmentStatus.Cancelled);
            var completedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Completed);

            var model = new AppointmentListViewModel
            {
                Appointments = appointments,
                FilterDate = filterDate,
                FilterStatus = filterStatus,
                FilterClient = filterClient,
                TotalAppointments = totalAppointments,
                TodayAppointments = todayAppointments,
                UpcomingAppointments = upcomingAppointments,
                CompletedAppointments = completedAppointments
            };

            return View(model);
        }

        // GET: Appointments/Calendar
        public async Task<IActionResult> Calendar(DateTime? month)
        {
            var currentMonth = month ?? DateTime.Today.AddDays(1 - DateTime.Today.Day);
            var startDate = currentMonth.AddDays(1 - currentMonth.Day);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get appointments for the month
            var appointments = await _context.Appointments
                .Include(a => a.CreatedByUser)
                .Where(a => a.AppointmentDate.Date >= startDate && a.AppointmentDate.Date <= endDate)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            // Create calendar days
            var days = new List<CalendarDay>();
            var calendarStart = startDate.AddDays(-(int)startDate.DayOfWeek);
            
            for (int i = 0; i < 42; i++) // 6 weeks * 7 days
            {
                var date = calendarStart.AddDays(i);
                var dayAppointments = appointments.Where(a => a.AppointmentDate.Date == date).ToList();
                
                days.Add(new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == currentMonth.Month,
                    IsToday = date.Date == DateTime.Today,
                    Appointments = dayAppointments
                });
            }

            var model = new AppointmentCalendarViewModel
            {
                CurrentMonth = currentMonth,
                Appointments = appointments,
                Days = days
            };

            return View(model);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Create history entries (basic implementation)
            var history = new List<AppointmentHistoryEntry>
            {
                new AppointmentHistoryEntry
                {
                    Date = appointment.CreatedAt,
                    Action = "Programare creat?",
                    Details = $"Programare creat? pentru {appointment.ClientName}",
                    UserId = appointment.CreatedByUserId,
                    UserName = appointment.CreatedByUser?.UserName
                }
            };

            if (appointment.UpdatedAt.HasValue)
            {
                history.Add(new AppointmentHistoryEntry
                {
                    Date = appointment.UpdatedAt.Value,
                    Action = "Programare modificat?",
                    Details = "Detaliile program?rii au fost actualizate"
                });
            }

            var model = new AppointmentDetailsViewModel
            {
                Appointment = appointment,
                History = history.OrderByDescending(h => h.Date).ToList()
            };

            return View(model);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            var model = new CreateAppointmentViewModel
            {
                AppointmentDate = DateTime.Today.AddHours(9) // Default to 9 AM today
            };
            
            return View(model);
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check for scheduling conflicts
                var hasConflict = await _context.Appointments
                    .AnyAsync(a => a.AppointmentDate < model.AppointmentDate.AddMinutes(model.DurationMinutes) &&
                                  a.AppointmentDate.AddMinutes(a.DurationMinutes) > model.AppointmentDate &&
                                  a.Status != AppointmentStatus.Cancelled);

                if (hasConflict)
                {
                    ModelState.AddModelError("AppointmentDate", "Exist? un conflict de programare în intervalul selectat.");
                    return View(model);
                }

                var currentUser = await _userManager.GetUserAsync(User);
                
                var appointment = new Appointment
                {
                    Title = model.Title,
                    Description = model.Description,
                    AppointmentDate = model.AppointmentDate,
                    DurationMinutes = model.DurationMinutes,
                    ClientName = model.ClientName,
                    ClientPhone = model.ClientPhone,
                    ClientEmail = model.ClientEmail,
                    Location = model.Location,
                    Status = model.Status,
                    Notes = model.Notes,
                    CreatedByUserId = currentUser?.Id,
                    CreatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Appointment created: {appointment.Title} for {appointment.ClientName} by {currentUser?.UserName}");

                TempData["Success"] = "Programarea a fost creat? cu succes!";
                return RedirectToAction(nameof(Details), new { id = appointment.Id });
            }

            return View(model);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var model = new EditAppointmentViewModel
            {
                Id = appointment.Id,
                Title = appointment.Title,
                Description = appointment.Description,
                AppointmentDate = appointment.AppointmentDate,
                DurationMinutes = appointment.DurationMinutes,
                ClientName = appointment.ClientName,
                ClientPhone = appointment.ClientPhone,
                ClientEmail = appointment.ClientEmail,
                Location = appointment.Location,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };

            return View(model);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAppointmentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    // Check for scheduling conflicts (exclude current appointment)
                    var hasConflict = await _context.Appointments
                        .AnyAsync(a => a.Id != id &&
                                      a.AppointmentDate < model.AppointmentDate.AddMinutes(model.DurationMinutes) &&
                                      a.AppointmentDate.AddMinutes(a.DurationMinutes) > model.AppointmentDate &&
                                      a.Status != AppointmentStatus.Cancelled);

                    if (hasConflict)
                    {
                        ModelState.AddModelError("AppointmentDate", "Exist? un conflict de programare în intervalul selectat.");
                        return View(model);
                    }

                    // Update appointment
                    appointment.Title = model.Title;
                    appointment.Description = model.Description;
                    appointment.AppointmentDate = model.AppointmentDate;
                    appointment.DurationMinutes = model.DurationMinutes;
                    appointment.ClientName = model.ClientName;
                    appointment.ClientPhone = model.ClientPhone;
                    appointment.ClientEmail = model.ClientEmail;
                    appointment.Location = model.Location;
                    appointment.Status = model.Status;
                    appointment.Notes = model.Notes;
                    appointment.UpdatedAt = DateTime.Now;

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    var currentUser = await _userManager.GetUserAsync(User);
                    _logger.LogInformation($"Appointment updated: {appointment.Title} by {currentUser?.UserName}");

                    TempData["Success"] = "Programarea a fost actualizat? cu succes!";
                    return RedirectToAction(nameof(Details), new { id = appointment.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(model);
        }

        // POST: Appointments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Programarea nu a fost g?sit?." });
            }

            try
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Appointment deleted: {appointment.Title} by {currentUser?.UserName}");

                return Json(new { success = true, message = "Programarea a fost ?tears? cu succes." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
                return Json(new { success = false, message = "A ap?rut o eroare la ?tergerea program?rii." });
            }
        }

        // POST: Appointments/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Programarea nu a fost g?sit?." });
            }

            try
            {
                appointment.Status = status;
                appointment.UpdatedAt = DateTime.Now;

                _context.Update(appointment);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                _logger.LogInformation($"Appointment status updated: {appointment.Title} to {status} by {currentUser?.UserName}");

                return Json(new { success = true, message = "Statusul program?rii a fost actualizat cu succes." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status {AppointmentId}", id);
                return Json(new { success = false, message = "A ap?rut o eroare la actualizarea statusului." });
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
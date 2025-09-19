using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            // Verificam daca utilizatorul este admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Statistici pentru dashboard
            var totalUsers = await _userManager.Users.CountAsync();
            var confirmedUsers = await _userManager.Users.CountAsync(u => u.EmailConfirmed);
            var unconfirmedUsers = totalUsers - confirmedUsers;
            
            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToListAsync();

            // Appointment statistics
            var totalAppointments = await _context.Appointments.CountAsync();
            var todayAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Date == DateTime.Today);
            var upcomingAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate > DateTime.Now && a.Status != AppointmentStatus.Cancelled);
            var completedAppointments = await _context.Appointments
                .CountAsync(a => a.Status == AppointmentStatus.Completed);
                
            var recentAppointments = await _context.Appointments
                .Include(a => a.CreatedByUser)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ConfirmedUsers = confirmedUsers,
                UnconfirmedUsers = unconfirmedUsers,
                RecentUsers = recentUsers,
                TotalAppointments = totalAppointments,
                TodayAppointments = todayAppointments,
                UpcomingAppointments = upcomingAppointments,
                CompletedAppointments = completedAppointments,
                RecentAppointments = recentAppointments
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = await _userManager.Users.ToListAsync();
            
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmUserEmail(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return Json(new { success = false, message = "Acces interzis" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Utilizatorul nu a fost gasit" });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Admin {currentUser.UserName} confirmed email for user {user.UserName}");
                return Json(new { success = true, message = "Email confirmat cu succes" });
            }

            return Json(new { success = false, message = "Eroare la confirmarea emailului" });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return Json(new { success = false, message = "Acces interzis" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Utilizatorul nu a fost gasit" });
            }

            if (user.Id == currentUser.Id)
            {
                return Json(new { success = false, message = "Nu te poti sterge pe tine insuti" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Admin {currentUser.UserName} deleted user {user.UserName}");
                return Json(new { success = true, message = "Utilizator sters cu succes" });
            }

            return Json(new { success = false, message = "Eroare la stergerea utilizatorului" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SystemInfo()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var model = new SystemInfoViewModel
            {
                ApplicationName = "Miseda Inspect SRL - Notificari Clienti",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ServerTime = DateTime.Now,
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet / 1024 / 1024, // MB
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync()
            };

            return View(model);
        }
    }
}
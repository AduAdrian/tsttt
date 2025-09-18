using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

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

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ConfirmedUsers = confirmedUsers,
                UnconfirmedUsers = unconfirmedUsers,
                RecentUsers = recentUsers
            };

            return View(model);
        }

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
                TotalUsers = await _userManager.Users.CountAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> InitializeAdmin()
        {
            // Verifica daca exista deja un admin
            if (await _roleManager.RoleExistsAsync("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Any())
                {
                    ViewBag.Message = "Sistemul are deja un administrator.";
                    return View();
                }
            }

            // Creaza rolul de admin daca nu exista
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            ViewBag.Message = "Poti crea primul administrator al sistemului.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string username, string email, string phoneNumber, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Parolele nu coincid.");
                return View("InitializeAdmin");
            }

            // Verifica daca exista deja un admin
            if (await _roleManager.RoleExistsAsync("Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Any())
                {
                    ViewBag.Error = "Sistemul are deja un administrator.";
                    return View("InitializeAdmin");
                }
            }

            var adminUser = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true // Adminul nu trebuie sa confirme emailul
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (result.Succeeded)
            {
                // Creaza rolul daca nu exista
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Adauga utilizatorul la rolul de admin
                await _userManager.AddToRoleAsync(adminUser, "Admin");

                _logger.LogInformation($"First admin created: {username}");
                ViewBag.Success = $"Administratorul '{username}' a fost creat cu succes! Te poti conecta acum.";
                return View("InitializeAdmin");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("InitializeAdmin");
        }
    }
}
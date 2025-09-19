using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<AccountController> _logger;
        private readonly IMemoryCache _cache;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<AccountController> logger,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string loginIdentifier, string password, bool rememberMe = false, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid && !string.IsNullOrEmpty(loginIdentifier) && !string.IsNullOrEmpty(password))
            {
                // Cauta utilizatorul dupa username, email sau numar de telefon
                var user = await FindUserByIdentifier(loginIdentifier);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Utilizator sau parol? incorect?");
                    _logger.LogWarning("Login attempt failed - user not found: {LoginIdentifier}", loginIdentifier);
                    return View();
                }

                // Verifica daca emailul este confirmat
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Contul '{user.UserName}' nu a fost confirmat prin email. Verific? emailul {user.Email} pentru linkul de confirmare.");
                    ViewBag.ShowResendLink = true;
                    ViewBag.Email = user.Email;
                    ViewBag.Username = user.UserName;
                    _logger.LogWarning("Login attempt failed - email not confirmed: {UserName}", user.UserName);
                    return View();
                }

                // Incearca autentificarea cu username-ul gasit
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in successfully: {UserName}", user.UserName);
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Contul '{user.UserName}' este blocat temporar din cauza prea multor încerc?ri e?uate. Încearc? din nou mai târziu.");
                    _logger.LogWarning("Login attempt failed - account locked out: {UserName}", user.UserName);
                    return View();
                }
                
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Contul '{user.UserName}' nu este autorizat s? se conecteze. Contacteaz? administratorul.");
                    _logger.LogWarning("Login attempt failed - not allowed: {UserName}", user.UserName);
                    return View();
                }
                
                // Parola gresita - mesaj generic pentru securitate
                ModelState.AddModelError(string.Empty, "Utilizator sau parol? incorect?");
                _logger.LogWarning("Login attempt failed - wrong password: {UserName}", user.UserName);
                return View();
            }

            ModelState.AddModelError(string.Empty, "Te rug?m s? completezi toate câmpurile.");
            return View();
        }

        private async Task<ApplicationUser?> FindUserByIdentifier(string identifier)
        {
            // Normalizeaz? identifier-ul
            var normalizedIdentifier = identifier.Trim();
            
            // Cauta dupa username
            var user = await _userManager.FindByNameAsync(normalizedIdentifier);
            if (user != null) return user;

            // Cauta dupa email
            user = await _userManager.FindByEmailAsync(normalizedIdentifier.ToLower());
            if (user != null) return user;

            // Cauta dupa numar de telefon - folosind NormalizePhoneNumber
            var phoneNumber = NormalizePhoneNumber(identifier);
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var users = await _userManager.Users
                    .Where(u => u.PhoneNumber == phoneNumber)
                    .FirstOrDefaultAsync();
                return users;
            }
            
            return null;
        }

        private string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
            
            // Elimin? spa?iile ?i caracterele speciale
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Format românesc: 07XXXXXXXX
            if (cleaned.StartsWith("07") && cleaned.Length == 10)
            {
                return cleaned;
            }
            
            // Format interna?ional: +407XXXXXXXX sau 407XXXXXXXX
            if (cleaned.StartsWith("407") && cleaned.Length == 12)
            {
                return "0" + cleaned.Substring(2);
            }
            
            return null;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string phoneNumber, string password, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                if (password != confirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Parolele nu coincid.");
                    return View();
                }

                var validationErrors = ValidateRegistrationInputs(username, email, phoneNumber, password);
                if (validationErrors.Count > 0)
                {
                    foreach (var error in validationErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View();
                }

                // Verific? unicitatea datelor
                var duplicateErrors = await CheckForDuplicatesAsync(username, email, phoneNumber);
                if (duplicateErrors.Count > 0)
                {
                    foreach (var error in duplicateErrors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View();
                }

                var user = new ApplicationUser 
                { 
                    UserName = username.Trim(),
                    Email = email.Trim().ToLower(),
                    PhoneNumber = NormalizePhoneNumber(phoneNumber),
                    DisplayName = username.Trim()
                };
                
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await SendEmailConfirmationAsync(user);
                    
                    ViewBag.Message = $"Cont creat cu succes pentru '{username}'! Am trimis un email de confirmare la {email}. Te rug?m s? confirmi emailul pentru a te putea conecta.";
                    ViewBag.Username = username;
                    ViewBag.Email = email;
                    return View("RegisterConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        private List<string> ValidateRegistrationInputs(string username, string email, string phoneNumber, string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 20)
            {
                errors.Add("Username-ul trebuie s? aib? între 3 ?i 20 de caractere.");
            }

            if (!IsValidEmail(email))
            {
                errors.Add("Formatul emailului nu este valid.");
            }

            if (NormalizePhoneNumber(phoneNumber) == null)
            {
                errors.Add("Num?rul de telefon trebuie s? fie în format românesc (07XXXXXXXX).");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                errors.Add("Parola trebuie s? aib? cel pu?in 6 caractere.");
            }

            return errors;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<string>> CheckForDuplicatesAsync(string username, string email, string phoneNumber)
        {
            var errors = new List<string>();

            // Verific? username
            if (await _userManager.FindByNameAsync(username.Trim()) != null)
            {
                errors.Add($"Username-ul '{username}' este deja folosit. Alege alt username.");
            }

            // Verific? email
            if (await _userManager.FindByEmailAsync(email.Trim().ToLower()) != null)
            {
                errors.Add($"Emailul '{email}' este deja folosit. Alege alt email.");
            }

            // Verific? telefon
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (!string.IsNullOrEmpty(normalizedPhone))
            {
                var existingUser = await _userManager.Users
                    .Where(u => u.PhoneNumber == normalizedPhone)
                    .FirstOrDefaultAsync();
                    
                if (existingUser != null)
                {
                    errors.Add($"Num?rul de telefon '{phoneNumber}' este deja folosit. Alege alt num?r.");
                }
            }

            return errors;
        }

        private async Task SendEmailConfirmationAsync(ApplicationUser user)
        {
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                    new { token = token, email = user.Email }, Request.Scheme);

                if (!string.IsNullOrEmpty(confirmationLink))
                {
                    await _emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
                    _logger.LogInformation("Email confirmation sent to {Email} for user {Username}", user.Email, user.UserName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Link invalid pentru confirmarea emailului.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Error = "Utilizatorul nu a fost g?sit.";
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Message = $"Email confirmat cu succes pentru '{user.UserName}'! Acum te po?i conecta la cont.";
                ViewBag.Username = user.UserName;
                _logger.LogInformation("Email confirmed successfully for {Email} (username: {Username})", email, user.UserName);
            }
            else
            {
                ViewBag.Error = "Eroare la confirmarea emailului. Link-ul poate fi expirat.";
                _logger.LogError("Failed to confirm email for {Email}: {Errors}", email, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(string email, string username)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.UserName != username)
            {
                ViewBag.Error = "Utilizatorul nu a fost g?sit.";
                return RedirectToAction("Login");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.Message = $"Emailul pentru '{username}' este deja confirmat. Te po?i conecta la cont.";
                return RedirectToAction("Login");
            }

            try
            {
                await SendEmailConfirmationAsync(user);
                ViewBag.Message = $"Am retrimis emailul de confirmare pentru '{username}' la {email}. Verific? inbox-ul ?i spam-ul.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend confirmation email to {Email}", email);
                ViewBag.Error = "Nu am putut retrimite emailul de confirmare. Încearc? din nou mai târziu.";
            }

            return View("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Te rug?m s? introduci o adres? de email valid?.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                try
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetLink = Url.Action("ResetPassword", "Account", 
                        new { token = token, email = email }, Request.Scheme);

                    if (!string.IsNullOrEmpty(resetLink))
                    {
                        await _emailService.SendPasswordResetEmailAsync(email, resetLink);
                        _logger.LogInformation("Password reset email sent to {Email} for user {Username}", email, user.UserName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                }
            }

            // Pentru securitate, afi??m acela?i mesaj indiferent de rezultat
            ViewBag.Message = "Dac? emailul exist? în sistem, vei primi un link de resetare.";
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordSms()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordSms(string phoneNumber)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(phoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Te rug?m s? introduci un num?r de telefon valid.");
                return View();
            }

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (normalizedPhone == null)
            {
                ModelState.AddModelError(string.Empty, "Formatul num?rului de telefon nu este valid.");
                return View();
            }

            var user = await _userManager.Users
                .Where(u => u.PhoneNumber == normalizedPhone)
                .FirstOrDefaultAsync();

            if (user != null && await _userManager.IsEmailConfirmedAsync(user))
            {
                try
                {
                    var smsCode = GenerateSecureSmsCode();
                    var cacheKey = $"sms_reset_{normalizedPhone}";
                    
                    // Stocheaz? codul în cache pentru 10 minute
                    _cache.Set(cacheKey, smsCode, TimeSpan.FromMinutes(10));
                    
                    await _smsService.SendPasswordResetSmsAsync(normalizedPhone, smsCode);
                    
                    ViewBag.PhoneNumber = normalizedPhone;
                    ViewBag.Username = user.UserName;
                    return View("VerifySmsCode");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", normalizedPhone);
                    ViewBag.Error = "A ap?rut o eroare la trimiterea SMS-ului. Te rug?m s? încerci din nou.";
                }
            }
            else
            {
                // Pentru securitate, simul?m acela?i flow
                await Task.Delay(Random.Shared.Next(1000, 3000));
            }

            ViewBag.Message = "Dac? num?rul de telefon exist? în sistem, vei primi un cod SMS.";
            return View();
        }

        private string GenerateSecureSmsCode()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 900000 + 100000).ToString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifySmsCode(string phoneNumber, string smsCode, string newPassword, string confirmPassword)
        {
            ViewBag.PhoneNumber = phoneNumber;

            if (string.IsNullOrWhiteSpace(smsCode) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Toate câmpurile sunt obligatorii.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Parolele nu coincid.");
                return View();
            }

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (normalizedPhone == null)
            {
                ModelState.AddModelError(string.Empty, "Num?rul de telefon nu este valid.");
                return View();
            }

            var cacheKey = $"sms_reset_{normalizedPhone}";
            if (!_cache.TryGetValue(cacheKey, out string? cachedCode) || cachedCode != smsCode)
            {
                ModelState.AddModelError(string.Empty, "Codul introdus este incorect sau expirat.");
                return View();
            }

            var user = await _userManager.Users
                .Where(u => u.PhoneNumber == normalizedPhone)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _cache.Remove(cacheKey); // ?terge codul folosit
                    ViewBag.Username = user.UserName;
                    _logger.LogInformation("Password reset successful via SMS for user {Username}", user.UserName);
                    return View("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Utilizatorul nu a fost g?sit.");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string email, string password, string confirmPassword)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Token = token;
                ViewBag.Email = email;
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Parolele nu coincid.");
                ViewBag.Token = token;
                ViewBag.Email = email;
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (result.Succeeded)
            {
                ViewBag.Success = $"Parola pentru contul '{user.UserName}' a fost resetat? cu succes. Te po?i conecta cu noua parol?.";
                ViewBag.Username = user.UserName;
                _logger.LogInformation("Password reset successful via email for user {Username}", user.UserName);
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out: {UserName}", User.Identity?.Name);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
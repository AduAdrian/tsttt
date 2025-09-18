using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<AccountController> _logger;

        // Dictionary pentru stocarea temporara a codurilor SMS (in productie ar trebui sa fie in cache/database)
        private static readonly Dictionary<string, (string code, DateTime expiry)> _smsCodes = new();

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string loginIdentifier, string password, bool rememberMe = false, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid && !string.IsNullOrEmpty(loginIdentifier) && !string.IsNullOrEmpty(password))
            {
                // Cauta utilizatorul dupa username, email sau numar de telefon
                var user = await FindUserByIdentifier(loginIdentifier);
                
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User sau parola incorecta");
                    _logger.LogWarning($"Login attempt failed - user not found: {loginIdentifier}");
                    return View();
                }

                // Verifica daca emailul este confirmat
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Contul '{user.UserName}' nu a fost confirmat prin email. Verifica emailul {user.Email} pentru linkul de confirmare.");
                    ViewBag.ShowResendLink = true;
                    ViewBag.Email = user.Email;
                    ViewBag.Username = user.UserName;
                    _logger.LogWarning($"Login attempt failed - email not confirmed: {user.UserName}");
                    return View();
                }

                // Incearca autentificarea cu username-ul gasit
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User logged in successfully: {user.UserName}");
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Contul '{user.UserName}' este blocat temporar din cauza prea multor incercari esuate. Incearca din nou mai tarziu.");
                    _logger.LogWarning($"Login attempt failed - account locked out: {user.UserName}");
                    return View();
                }
                
                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, 
                        $"Contul '{user.UserName}' nu este autorizat sa se conecteze. Contacteaza administratorul.");
                    _logger.LogWarning($"Login attempt failed - not allowed: {user.UserName}");
                    return View();
                }
                
                // Parola gresita - mesaj generic pentru securitate
                ModelState.AddModelError(string.Empty, "User sau parola incorecta");
                _logger.LogWarning($"Login attempt failed - wrong password: {user.UserName}");
                return View();
            }

            ModelState.AddModelError(string.Empty, "Te rugam sa completezi toate campurile.");
            return View();
        }

        private async Task<ApplicationUser?> FindUserByIdentifier(string identifier)
        {
            // Cauta dupa username
            var user = await _userManager.FindByNameAsync(identifier);
            if (user != null) return user;

            // Cauta dupa email
            user = await _userManager.FindByEmailAsync(identifier);
            if (user != null) return user;

            // Cauta dupa numar de telefon
            var users = _userManager.Users.Where(u => u.PhoneNumber == identifier).ToList();
            return users.FirstOrDefault();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string phoneNumber, string password, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                if (password != confirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Parolele nu coincid.");
                    return View();
                }

                if (string.IsNullOrEmpty(username))
                {
                    ModelState.AddModelError(string.Empty, "Username-ul este obligatoriu.");
                    return View();
                }

                if (string.IsNullOrEmpty(email))
                {
                    ModelState.AddModelError(string.Empty, "Email-ul este obligatoriu.");
                    return View();
                }

                if (string.IsNullOrEmpty(phoneNumber))
                {
                    ModelState.AddModelError(string.Empty, "Numarul de telefon este obligatoriu.");
                    return View();
                }

                // Verifica daca username-ul exista deja
                var existingUser = await _userManager.FindByNameAsync(username);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, $"Username-ul '{username}' este deja folosit. Alege alt username.");
                    return View();
                }

                // Verifica daca emailul exista deja
                existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, $"Emailul '{email}' este deja folosit. Alege alt email.");
                    return View();
                }

                // Verifica daca numarul de telefon exista deja
                var existingUsers = _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).ToList();
                if (existingUsers.Any())
                {
                    ModelState.AddModelError(string.Empty, $"Numarul de telefon '{phoneNumber}' este deja folosit. Alege alt numar.");
                    return View();
                }

                var user = new ApplicationUser 
                { 
                    UserName = username,
                    Email = email,
                    PhoneNumber = phoneNumber
                };
                
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Generare token de confirmare email
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                        new { token = token, email = email }, Request.Scheme);

                    if (!string.IsNullOrEmpty(confirmationLink))
                    {
                        try
                        {
                            await _emailService.SendEmailConfirmationAsync(email, confirmationLink);
                            _logger.LogInformation($"Email confirmation sent to {email} for user {username}");
                            
                            ViewBag.Message = $"Cont creat cu succes pentru '{username}'! Am trimis un email de confirmare la {email}. Te rugam sa confirmi emailul pentru a te putea conecta.";
                            ViewBag.Username = username;
                            ViewBag.Email = email;
                            return View("RegisterConfirmation");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send confirmation email to {email}");
                            ViewBag.Error = "Contul a fost creat dar nu am putut trimite emailul de confirmare. Contacteaza administratorul.";
                        }
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
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
                ViewBag.Error = "Utilizatorul nu a fost gasit.";
                return View();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.Message = $"Email confirmat cu succes pentru '{user.UserName}'! Acum te poti conecta la cont.";
                ViewBag.Username = user.UserName;
                _logger.LogInformation($"Email confirmed successfully for {email} (username: {user.UserName})");
            }
            else
            {
                ViewBag.Error = "Eroare la confirmarea emailului. Link-ul poate fi expirat.";
                _logger.LogError($"Failed to confirm email for {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(string email, string username)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.UserName != username)
            {
                ViewBag.Error = "Utilizatorul nu a fost gasit.";
                return RedirectToAction("Login");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.Message = $"Emailul pentru '{username}' este deja confirmat. Te poti conecta la cont.";
                return RedirectToAction("Login");
            }

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                    new { token = token, email = email }, Request.Scheme);

                if (!string.IsNullOrEmpty(confirmationLink))
                {
                    await _emailService.SendEmailConfirmationAsync(email, confirmationLink);
                    _logger.LogInformation($"Email confirmation resent to {email} for user {username}");
                    
                    ViewBag.Message = $"Am retrimis emailul de confirmare pentru '{username}' la {email}. Verifica inbox-ul si spam-ul.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to resend confirmation email to {email}");
                ViewBag.Error = "Nu am putut retrimite emailul de confirmare. Incearca din nou mai tarziu.";
            }

            return View("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    // Verificam daca emailul este confirmat
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ViewBag.Error = $"Emailul pentru contul '{user.UserName}' nu este confirmat. Nu poti reseta parola pentru un cont neconfirmat.";
                        return View();
                    }

                    // Generare token pentru resetarea parolei
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    
                    // Creare link pentru resetarea parolei
                    var resetLink = Url.Action("ResetPassword", "Account", 
                        new { token = token, email = email }, Request.Scheme);

                    if (!string.IsNullOrEmpty(resetLink))
                    {
                        try
                        {
                            await _emailService.SendPasswordResetEmailAsync(email, resetLink);
                            ViewBag.Message = $"Un link de resetare a parolei a fost trimis pentru contul '{user.UserName}' la emailul {email}.";
                            _logger.LogInformation($"Password reset email sent to {email} for user {user.UserName}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send password reset email to {email}");
                            ViewBag.Error = "A aparut o eroare la trimiterea emailului. Te rugam sa incerci din nou.";
                        }
                    }
                }
                else
                {
                    // Pentru securitate, afisam acelasi mesaj chiar daca emailul nu exista
                    ViewBag.Message = "Daca emailul exista in sistem, vei primi un link de resetare.";
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Te rugam sa introduci o adresa de email valida.");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordSms()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordSms(string phoneNumber)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(phoneNumber))
            {
                // Cautam user-ul dupa numarul de telefon
                var users = _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).ToList();
                var user = users.FirstOrDefault();

                if (user != null)
                {
                    // Verificam daca emailul este confirmat
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        ViewBag.Error = $"Contul '{user.UserName}' nu este confirmat prin email. Nu poti reseta parola pentru un cont neconfirmat.";
                        return View();
                    }

                    try
                    {
                        // Generare cod de 6 cifre
                        var random = new Random();
                        var smsCode = random.Next(100000, 999999).ToString();
                        
                        // Salvare cod cu expirare de 10 minute
                        _smsCodes[phoneNumber] = (smsCode, DateTime.Now.AddMinutes(10));
                        
                        // Trimitere SMS
                        await _smsService.SendPasswordResetSmsAsync(phoneNumber, smsCode);
                        
                        ViewBag.PhoneNumber = phoneNumber;
                        ViewBag.Username = user.UserName;
                        return View("VerifySmsCode");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to send SMS to {phoneNumber}");
                        ViewBag.Error = "A aparut o eroare la trimiterea SMS-ului. Te rugam sa incerci din nou.";
                    }
                }
                else
                {
                    // Pentru securitate, afisam acelasi mesaj chiar daca numarul nu exista
                    ViewBag.Message = "Daca numarul de telefon exista in sistem, vei primi un cod SMS.";
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Te rugam sa introduci un numar de telefon valid.");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifySmsCode(string phoneNumber, string smsCode, string newPassword, string confirmPassword)
        {
            ViewBag.PhoneNumber = phoneNumber;

            if (string.IsNullOrEmpty(smsCode) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Toate campurile sunt obligatorii.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Parolele nu coincid.");
                return View();
            }

            // Verificare cod SMS
            if (_smsCodes.TryGetValue(phoneNumber, out var storedCodeData))
            {
                if (DateTime.Now > storedCodeData.expiry)
                {
                    _smsCodes.Remove(phoneNumber);
                    ModelState.AddModelError(string.Empty, "Codul a expirat. Te rugam sa soliciti un cod nou.");
                    return RedirectToAction("ForgotPasswordSms");
                }

                if (storedCodeData.code == smsCode)
                {
                    // Cod corect, resetam parola
                    var users = _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).ToList();
                    var user = users.FirstOrDefault();

                    if (user != null)
                    {
                        // Generare token pentru resetare
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                        if (result.Succeeded)
                        {
                            _smsCodes.Remove(phoneNumber); // Stergem codul folosit
                            ViewBag.Username = user.UserName;
                            _logger.LogInformation($"Password reset successful via SMS for user {user.UserName}");
                            return View("ResetPasswordConfirmation");
                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Codul introdus este incorect.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Cod invalid sau expirat.");
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
                ViewBag.Success = $"Parola pentru contul '{user.UserName}' a fost resetata cu succes. Te poti conecta cu noua parola.";
                ViewBag.Username = user.UserName;
                _logger.LogInformation($"Password reset successful via email for user {user.UserName}");
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
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<TestController> _logger;

        public TestController(IEmailService emailService, ISmsService smsService, ILogger<TestController> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult TestEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestEmail(string testEmail)
        {
            if (string.IsNullOrEmpty(testEmail))
            {
                ViewBag.Error = "Te rugam sa introduci un email valid pentru test.";
                return View();
            }

            try
            {
                var testLink = Url.Action("ResetPassword", "Account", 
                    new { token = "test-token-12345", email = testEmail }, Request.Scheme);
                
                await _emailService.SendPasswordResetEmailAsync(testEmail, testLink ?? "");
                
                ViewBag.Message = $"Email de test trimis cu succes catre: {testEmail}";
                _logger.LogInformation($"Email de test trimis cu succes catre: {testEmail}");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Eroare la trimiterea emailului: {ex.Message}";
                _logger.LogError(ex, $"Eroare la trimiterea emailului de test catre: {testEmail}");
            }

            return View();
        }

        [HttpGet]
        public IActionResult TestSms()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestSms(string testPhone)
        {
            if (string.IsNullOrEmpty(testPhone))
            {
                ViewBag.Error = "Te rugam sa introduci un numar de telefon valid pentru test.";
                return View();
            }

            try
            {
                var testCode = "123456";
                await _smsService.SendPasswordResetSmsAsync(testPhone, testCode);
                
                ViewBag.Message = $"SMS de test trimis cu succes catre: {testPhone}";
                _logger.LogInformation($"SMS de test trimis cu succes catre: {testPhone}");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Eroare la trimiterea SMS-ului: {ex.Message}";
                _logger.LogError(ex, $"Eroare la trimiterea SMS-ului de test catre: {testPhone}");
            }

            return View();
        }

        [HttpGet]
        public IActionResult NavigationTest()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("NavigationTest");
        }
    }
}
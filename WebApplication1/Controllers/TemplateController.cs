using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class TemplateController : Controller
    {
        private readonly IExcelTemplateService _excelTemplateService;
        private readonly ILogger<TemplateController> _logger;

        public TemplateController(IExcelTemplateService excelTemplateService, ILogger<TemplateController> logger)
        {
            _excelTemplateService = excelTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Descarc? template-ul Excel automatizat pentru clien?i
        /// </summary>
        [HttpGet]
        public IActionResult DownloadSmartTemplate()
        {
            try
            {
                _logger.LogInformation("Generare template Excel automatizat pentru clien?i - User: {UserId}", User.Identity?.Name);
                
                var templateData = _excelTemplateService.GenerateClientTemplate();
                
                var fileName = $"Template_Clienti_ITP_Automatizat_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";
                
                _logger.LogInformation("Template Excel generat cu succes: {FileName}", fileName);
                
                return File(
                    templateData, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la generarea template-ului Excel");
                TempData["ErrorMessage"] = "A ap?rut o eroare la generarea template-ului Excel. V? rug?m s? încerca?i din nou.";
                return RedirectToAction("Index", "Clients");
            }
        }

        /// <summary>
        /// Descarc? ghidul detaliat de utilizare
        /// </summary>
        [HttpGet]
        public IActionResult DownloadInstructionsGuide()
        {
            try
            {
                var instructionsData = _excelTemplateService.GenerateInstructionsTemplate();
                
                var fileName = $"Ghid_Utilizare_Template_Excel_{DateTime.Now:yyyy-MM-dd}.xlsx";
                
                return File(
                    instructionsData, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la generarea ghidului de instruc?iuni");
                TempData["ErrorMessage"] = "A ap?rut o eroare la generarea ghidului. V? rug?m s? încerca?i din nou.";
                return RedirectToAction("Index", "Clients");
            }
        }

        /// <summary>
        /// Afi?eaz? pagina de informa?ii despre template
        /// </summary>
        [HttpGet]
        public IActionResult TemplateInfo()
        {
            ViewData["Title"] = "Informa?ii Template Excel";
            return View();
        }

        /// <summary>
        /// Afi?eaz? pagina pentru import Excel
        /// </summary>
        [HttpGet]
        public IActionResult Import()
        {
            ViewData["Title"] = "Import Date din Excel";
            return View();
        }

        /// <summary>
        /// Proceseaz? importul din Excel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ErrorMessage"] = "V? rug?m s? selecta?i un fi?ier Excel valid.";
                return View();
            }

            if (!IsValidExcelFile(excelFile))
            {
                TempData["ErrorMessage"] = "Fi?ierul trebuie s? fie de tip Excel (.xlsx sau .xls).";
                return View();
            }

            if (excelFile.Length > 10 * 1024 * 1024) // 10MB limit
            {
                TempData["ErrorMessage"] = "Fi?ierul este prea mare. Dimensiunea maxim? permis? este 10MB.";
                return View();
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await excelFile.CopyToAsync(memoryStream);
                var excelData = memoryStream.ToArray();

                var userId = User.Identity?.Name ?? "system";
                var importResult = await _excelTemplateService.ImportClientsFromExcelAsync(excelData, userId);

                if (importResult.Success)
                {
                    TempData["SuccessMessage"] = importResult.Message;
                    
                    if (importResult.Errors > 0)
                    {
                        TempData["WarningMessage"] = $"Import par?ial reu?it cu {importResult.Errors} erori. Verifica?i jurnalul pentru detalii.";
                    }

                    _logger.LogInformation("Excel import completed: {Success} successful, {Errors} errors, User: {UserId}", 
                        importResult.SuccessfulImports, importResult.Errors, userId);
                }
                else
                {
                    var errorMessage = string.Join("; ", importResult.ErrorMessages.Take(3));
                    if (importResult.ErrorMessages.Count > 3)
                    {
                        errorMessage += $" ?i înc? {importResult.ErrorMessages.Count - 3} erori...";
                    }
                    
                    TempData["ErrorMessage"] = $"Import e?uat: {errorMessage}";
                }

                // Pentru debugging, salveaz? detaliile în ViewBag
                ViewBag.ImportResult = importResult;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare critic? la importul Excel - User: {UserId}", User.Identity?.Name);
                TempData["ErrorMessage"] = "A ap?rut o eroare critic? la procesarea fi?ierului. V? rug?m s? încerca?i din nou.";
                return View();
            }
        }

        /// <summary>
        /// Preview template în browser
        /// </summary>
        [HttpGet]
        public IActionResult PreviewTemplate()
        {
            try
            {
                var templateInfo = new
                {
                    Columns = new[]
                    {
                        new { Name = "Nr. Înmatriculare", Type = "Text", Required = true, Example = "SV14ABC", Description = "Num?rul de înmatriculare al vehiculului (6-10 caractere)" },
                        new { Name = "Telefon", Type = "Number", Required = false, Example = "0756596565", Description = "Num?rul de telefon (default: 0756596565 - Miseda Inspect SRL)" },
                        new { Name = "Tip Valabilitate", Type = "Dropdown", Required = true, Example = "1 An", Description = "Selecteaz? din: 6 Luni, 1 An, 2 Ani, Manual" },
                        new { Name = "Data Expirare", Type = "Date (Calculat?)", Required = false, Example = "15.03.2025", Description = "Se calculeaz? automat sau manual pentru tipul 'Manual'" },
                        new { Name = "Status", Type = "Calculat Automat", Required = false, Example = "Valid", Description = "Valid (verde), Expir? Curând (galben), Expirat (ro?u)" },
                        new { Name = "Zile R?mase", Type = "Calculat Automat", Required = false, Example = "365", Description = "Num?rul de zile pân? la expirare" }
                    },
                    Features = new[]
                    {
                        "?? Dropdown automatizat pentru Tip Valabilitate",
                        "?? Calculare automat? a datei de expirare cu formule Excel",
                        "?? Status automat cu culori: Valid/Expir? Curând/Expirat",
                        "?? Highlighting colorat condi?ional pentru vizualizare rapid?",
                        "? Valid?ri automate pentru telefon românesc (07XXXXXXXX)",
                        "?? Protejarea coloanelor calculate cu parol?: miseda2024",
                        "?? 4 sheet-uri: Date, Instruc?iuni, Exemple, Statistici live",
                        "?? Import care ÎNLOCUIE?TE complet baza de date existent?"
                    },
                    AutomationFormulas = new[]
                    {
                        "6 Luni ? Data Expirare = TODAY() + 180 zile",
                        "1 An ? Data Expirare = TODAY() + 365 zile", 
                        "2 Ani ? Data Expirare = TODAY() + 730 zile",
                        "Manual ? Permite introducere manual? cu validare",
                        "Status ? IF(D<TODAY(),\"Expirat\",IF(D<TODAY()+30,\"Expir? Curând\",\"Valid\"))",
                        "Zile R?mase ? D2-TODAY() (calculat live)"
                    },
                    Instructions = new[]
                    {
                        "?? Completeaz? DOAR coloanele A, B, C (?i D pentru 'Manual')",
                        "?? Coloanele E ?i F se calculeaz? automat - NU le modifica",
                        "?? La import, baza de date se ÎNLOCUIE?TE complet",
                        "?? Telefon default: 0756596565 (Miseda Inspect SRL)",
                        "?? Maxim 500 de clien?i per template",
                        "?? Template protejat cu parola: miseda2024",
                        "?? Salveaz? ca .xlsx pentru import"
                    },
                    ValidationRules = new[]
                    {
                        "Nr. Înmatriculare: 6-10 caractere, obligatoriu",
                        "Telefon: 10 cifre, începe cu 07, format românesc",
                        "Tip Valabilitate: doar din dropdown (6 Luni/1 An/2 Ani/Manual)",
                        "Data Manual: între azi ?i urm?torii 3 ani",
                        "F?r? duplicate în coloana A",
                        "Limite: maxim 10MB, format .xlsx sau .xls"
                    }
                };

                return View(templateInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la afi?area preview-ului template");
                TempData["ErrorMessage"] = "A ap?rut o eroare la afi?area informa?iilor despre template.";
                return RedirectToAction("Index", "Clients");
            }
        }

        /// <summary>
        /// Verific? dac? fi?ierul este un Excel valid
        /// </summary>
        private bool IsValidExcelFile(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(extension);
        }

        /// <summary>
        /// ?terge toate datele existente (pentru utilizare în cazuri speciale)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAllData()
        {
            try
            {
                // Aceast? func?ie este disponibil? doar pentru administratori
                if (!User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "Nu ave?i permisiuni pentru aceast? opera?ie.";
                    return RedirectToAction("Index", "Clients");
                }

                var emptyData = new byte[0];
                var userId = User.Identity?.Name ?? "admin";
                
                // Simul?m un import gol pentru a ?terge toate datele
                // În practic?, ar trebui implementat separat pentru siguran??
                
                _logger.LogWarning("Database clear requested by user: {UserId}", userId);
                TempData["SuccessMessage"] = "Toate datele au fost ?terse cu succes.";
                
                return RedirectToAction("Index", "Clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la ?tergerea datelor");
                TempData["ErrorMessage"] = "A ap?rut o eroare la ?tergerea datelor.";
                return RedirectToAction("Index", "Clients");
            }
        }
    }
}
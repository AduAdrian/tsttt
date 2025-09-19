using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Text;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDataSeedingService _dataSeedingService;
        private const int MAX_IMPORT_ROWS = 1000; // Limita pentru import in masa
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB

        public ClientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IDataSeedingService dataSeedingService)
        {
            _context = context;
            _userManager = userManager;
            _dataSeedingService = dataSeedingService;
        }

        // GET: Clients
        public async Task<IActionResult> Index(ClientSearchViewModel searchModel)
        {
            // Seed data if empty
            if (!await _dataSeedingService.HasSeedDataAsync())
            {
                await _dataSeedingService.SeedClientsAsync();
            }

            // Validate page size
            if (searchModel.PageSize != 10 && searchModel.PageSize != 50)
            {
                searchModel.PageSize = 10;
            }

            // Build query
            var query = _context.Clients
                .Where(c => c.IsActive)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.RegistrationNumber.ToLower().Contains(searchTerm) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(searchTerm)) ||
                    c.ValidityType.ToString().ToLower().Contains(searchTerm)
                );
            }

            // Apply status filter
            var today = DateTime.Today;
            switch (searchModel.StatusFilter)
            {
                case ClientStatusFilter.Valid:
                    query = query.Where(c => c.ExpiryDate > today.AddDays(30));
                    break;
                case ClientStatusFilter.ExpiringSoon:
                    query = query.Where(c => c.ExpiryDate >= today && c.ExpiryDate <= today.AddDays(30));
                    break;
                case ClientStatusFilter.Expired:
                    query = query.Where(c => c.ExpiryDate < today);
                    break;
            }

            // Apply sorting
            query = searchModel.SortBy?.ToLower() switch
            {
                "registrationnumber" => searchModel.SortDirection == "desc" 
                    ? query.OrderByDescending(c => c.RegistrationNumber)
                    : query.OrderBy(c => c.RegistrationNumber),
                "validitytype" => searchModel.SortDirection == "desc"
                    ? query.OrderByDescending(c => c.ValidityType)
                    : query.OrderBy(c => c.ValidityType),
                "createdat" => searchModel.SortDirection == "desc"
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt),
                _ => searchModel.SortDirection == "desc"
                    ? query.OrderByDescending(c => c.ExpiryDate)
                    : query.OrderBy(c => c.ExpiryDate)
            };

            // Get total count for pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / searchModel.PageSize);

            // Ensure page is within bounds
            searchModel.Page = Math.Max(1, Math.Min(searchModel.Page, totalPages == 0 ? 1 : totalPages));

            // Get paginated data
            var clients = await query
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .Select(c => new ClientListViewModel
                {
                    Id = c.Id,
                    RegistrationNumber = c.RegistrationNumber,
                    ValidityType = c.ValidityType,
                    ExpiryDate = c.ExpiryDate,
                    PhoneNumber = c.PhoneNumber,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            // Calculate statistics for all clients (not just current page)
            var allClientsQuery = _context.Clients.Where(c => c.IsActive);
            
            // Apply search filter for statistics too
            if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                allClientsQuery = allClientsQuery.Where(c => 
                    c.RegistrationNumber.ToLower().Contains(searchTerm) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(searchTerm)) ||
                    c.ValidityType.ToString().ToLower().Contains(searchTerm)
                );
            }

            var statistics = await allClientsQuery
                .GroupBy(c => 1)
                .Select(g => new
                {
                    Expired = g.Count(c => c.ExpiryDate < today),
                    ExpiringSoon = g.Count(c => c.ExpiryDate >= today && c.ExpiryDate <= today.AddDays(30)),
                    Valid = g.Count(c => c.ExpiryDate > today.AddDays(30))
                })
                .FirstOrDefaultAsync();

            var viewModel = new PaginatedClientsViewModel
            {
                Clients = clients,
                CurrentPage = searchModel.Page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = searchModel.PageSize,
                SearchTerm = searchModel.SearchTerm,
                SortBy = searchModel.SortBy,
                SortDirection = searchModel.SortDirection,
                ExpiredCount = statistics?.Expired ?? 0,
                ExpiringSoonCount = statistics?.ExpiringSoon ?? 0,
                ValidCount = statistics?.Valid ?? 0
            };

            // Pass search model for form binding
            ViewBag.SearchModel = searchModel;

            return View(viewModel);
        }

        // POST: Seed test data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                await _dataSeedingService.SeedClientsAsync();
                TempData["SuccessMessage"] = "100 de clien?i test au fost genera?i cu succes!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Eroare la generarea datelor test: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Download Template Excel
        public IActionResult DownloadTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Template Clienti ITP");
            
            // Headers
            worksheet.Cells[1, 1].Value = "Nr. Inmatriculare";
            worksheet.Cells[1, 2].Value = "Numar Telefon";
            worksheet.Cells[1, 3].Value = "Tip Valabilitate";
            worksheet.Cells[1, 4].Value = "Data Expirare ITP";
            
            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            
            // Add example data with validation notes
            worksheet.Cells[2, 1].Value = "B123ABC";
            worksheet.Cells[2, 2].Value = "0756123456";
            worksheet.Cells[2, 3].Value = "Manual";
            worksheet.Cells[2, 4].Value = DateTime.Today.AddMonths(12);
            worksheet.Cells[2, 4].Style.Numberformat.Format = "dd/mm/yyyy";
            
            worksheet.Cells[3, 1].Value = "CJ456DEF";
            worksheet.Cells[3, 2].Value = "0765654321";
            worksheet.Cells[3, 3].Value = "6 Luni";
            worksheet.Cells[3, 4].Value = "(se calculeaza automat)";
            
            // Detailed validation instructions
            worksheet.Cells[5, 1].Value = "REGULI DE VALIDARE:";
            worksheet.Cells[5, 1].Style.Font.Bold = true;
            worksheet.Cells[5, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[5, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
            
            worksheet.Cells[6, 1].Value = "1. Nr. Inmatriculare:";
            worksheet.Cells[6, 2].Value = "• Format: 2-3 litere + 2-3 cifre + 3 litere (ex: B123ABC)";
            worksheet.Cells[7, 2].Value = "• Lungime maxima: 15 caractere";
            worksheet.Cells[8, 2].Value = "• Obligatoriu si unic";
            
            worksheet.Cells[9, 1].Value = "2. Numar Telefon:";
            worksheet.Cells[9, 2].Value = "• Format romanesc: 07XXXXXXXX sau +407XXXXXXXX";
            worksheet.Cells[10, 2].Value = "• Lungime: 10 cifre (fara prefix) sau 13 cu prefix";
            worksheet.Cells[11, 2].Value = "• Optional pentru SMS";
            
            worksheet.Cells[12, 1].Value = "3. Tip Valabilitate:";
            worksheet.Cells[12, 2].Value = "• Valori acceptate: Manual, 6 Luni, 1 An, 2 Ani";
            worksheet.Cells[13, 2].Value = "• Case insensitive (manual = Manual)";
            
            worksheet.Cells[14, 1].Value = "4. Data Expirare:";
            worksheet.Cells[14, 2].Value = "• Pentru Manual: DD/MM/YYYY sau DD-MM-YYYY";
            worksheet.Cells[15, 2].Value = "• Pentru altele: lasati gol (se calculeaza automat)";
            worksheet.Cells[16, 2].Value = "• Nu poate fi in trecut";
            
            worksheet.Cells[18, 1].Value = "LIMITE IMPORT:";
            worksheet.Cells[18, 1].Style.Font.Bold = true;
            worksheet.Cells[18, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[18, 1].Style.Fill.BackgroundColor.SetColor(Color.Orange);
            worksheet.Cells[19, 1].Value = "• Maxim 1000 randuri per import";
            worksheet.Cells[20, 1].Value = "• Marimea fisierului: maxim 5MB";
            worksheet.Cells[21, 1].Value = "• Format acceptat: doar .xlsx";
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Clienti_ITP.xlsx");
        }

        // GET: Export Excel cu toti clientii
        public async Task<IActionResult> ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            var clients = await _context.Clients
                .Where(c => c.IsActive)
                .OrderBy(c => c.ExpiryDate)
                .ToListAsync();
                
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Clienti ITP");
            
            // Headers
            worksheet.Cells[1, 1].Value = "Nr. Inmatriculare";
            worksheet.Cells[1, 2].Value = "Numar Telefon";
            worksheet.Cells[1, 3].Value = "Tip Valabilitate";
            worksheet.Cells[1, 4].Value = "Data Expirare ITP";
            worksheet.Cells[1, 5].Value = "Zile Ramase";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Data Crearii";
            
            // Format headers
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            
            // Add data
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                var row = i + 2;
                var daysUntilExpiry = (client.ExpiryDate - DateTime.Today).Days;
                
                worksheet.Cells[row, 1].Value = client.RegistrationNumber;
                worksheet.Cells[row, 2].Value = client.PhoneNumber ?? "Fara telefon";
                worksheet.Cells[row, 3].Value = client.ValidityType.GetDisplayName();
                worksheet.Cells[row, 4].Value = client.ExpiryDate;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "dd/mm/yyyy";
                worksheet.Cells[row, 5].Value = daysUntilExpiry;
                worksheet.Cells[row, 6].Value = GetStatusText(client.ExpiryDate);
                worksheet.Cells[row, 7].Value = client.CreatedAt;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "dd/mm/yyyy";
                
                // Color coding based on expiry
                if (daysUntilExpiry < 0)
                {
                    using (var range = worksheet.Cells[row, 1, row, 7])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                    }
                }
                else if (daysUntilExpiry <= 30)
                {
                    using (var range = worksheet.Cells[row, 1, row, 7])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                    }
                }
            }
            
            worksheet.Cells.AutoFitColumns();
            
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            
            var fileName = $"Clienti_ITP_{DateTime.Now:yyyy-MM-dd}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // GET: Export CSV cu toti clientii
        public async Task<IActionResult> ExportCsv()
        {
            var clients = await _context.Clients
                .Where(c => c.IsActive)
                .OrderBy(c => c.ExpiryDate)
                .ToListAsync();
                
            var csv = new StringBuilder();
            
            // Header CSV
            csv.AppendLine("Nr. Inmatriculare,Numar Telefon,Tip Valabilitate,Data Expirare ITP,Zile Ramase,Status,Data Crearii");
            
            // Add data
            foreach (var client in clients)
            {
                var daysUntilExpiry = (client.ExpiryDate - DateTime.Today).Days;
                var status = GetStatusText(client.ExpiryDate);
                var phoneNumber = string.IsNullOrEmpty(client.PhoneNumber) ? "Fara telefon" : client.PhoneNumber;
                
                csv.AppendLine($"\"{client.RegistrationNumber}\",\"{phoneNumber}\",\"{client.ValidityType.GetDisplayName()}\",\"{client.ExpiryDate:dd/MM/yyyy}\",{daysUntilExpiry},\"{status}\",\"{client.CreatedAt:dd/MM/yyyy}\"");
            }
            
            var fileName = $"Clienti_ITP_{DateTime.Now:yyyy-MM-dd}.csv";
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            
            return File(bytes, "text/csv", fileName);
        }

        private string GetStatusText(DateTime expiryDate)
        {
            var daysUntilExpiry = (expiryDate - DateTime.Today).Days;
            return daysUntilExpiry < 0 ? "Expirat" : daysUntilExpiry <= 30 ? "Expira curand" : "Valid";
        }

        // POST: Import Excel cu validari complete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MAX_FILE_SIZE)]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Va rugam selectati un fisier Excel.";
                return RedirectToAction(nameof(Index));
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Fisierul trebuie sa fie in format .xlsx";
                return RedirectToAction(nameof(Index));
            }

            if (file.Length > MAX_FILE_SIZE)
            {
                TempData["ErrorMessage"] = $"Fisierul este prea mare. Marimea maxima permisa: {MAX_FILE_SIZE / (1024 * 1024)}MB";
                return RedirectToAction(nameof(Index));
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            var successCount = 0;
            var errorCount = 0;
            var warnings = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    TempData["ErrorMessage"] = "Fisierul Excel nu contine worksheets valide.";
                    return RedirectToAction(nameof(Index));
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                {
                    TempData["ErrorMessage"] = "Fisierul Excel nu contine date (doar header).";
                    return RedirectToAction(nameof(Index));
                }

                if (rowCount > MAX_IMPORT_ROWS + 1)
                {
                    TempData["ErrorMessage"] = $"Fisierul contine prea multe randuri. Maxim permis: {MAX_IMPORT_ROWS}";
                    return RedirectToAction(nameof(Index));
                }

                var currentUserId = _userManager.GetUserId(User);

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var registrationNumber = worksheet.Cells[row, 1].Value?.ToString()?.Trim()?.ToUpper();
                        var phoneNumber = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var validityTypeText = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var expiryDateValue = worksheet.Cells[row, 4].Value;

                        if (string.IsNullOrEmpty(registrationNumber))
                        {
                            warnings.Add($"Rand {row}: Nr. inmatriculare este obligatoriu - ignorat");
                            errorCount++;
                            continue;
                        }

                        if (!IsValidRegistrationNumber(registrationNumber))
                        {
                            warnings.Add($"Rand {row}: Format nr. inmatriculare invalid - ignorat");
                            errorCount++;
                            continue;
                        }

                        // Parse ValidityType
                        ValidityType validityType = ValidityType.Manual;
                        if (!string.IsNullOrEmpty(validityTypeText))
                        {
                            validityType = validityTypeText.ToLower() switch
                            {
                                "manual" => ValidityType.Manual,
                                "6 luni" => ValidityType.SixMonths,
                                "1 an" => ValidityType.OneYear,
                                "2 ani" => ValidityType.TwoYears,
                                _ => ValidityType.Manual
                            };
                        }

                        // Calculate expiry date
                        DateTime expiryDate;
                        if (validityType == ValidityType.Manual)
                        {
                            if (expiryDateValue == null || !DateTime.TryParse(expiryDateValue.ToString(), out expiryDate))
                            {
                                warnings.Add($"Rand {row}: Data expirare invalida pentru tip Manual - ignorat");
                                errorCount++;
                                continue;
                            }
                        }
                        else
                        {
                            var months = (int)validityType;
                            expiryDate = DateTime.Today.AddMonths(months);
                        }

                        // Normalize phone number
                        if (!string.IsNullOrEmpty(phoneNumber))
                        {
                            phoneNumber = NormalizePhoneNumber(phoneNumber);
                            if (!IsValidPhoneNumber(phoneNumber))
                            {
                                phoneNumber = null;
                            }
                        }

                        // Check if client already exists
                        var existingClient = await _context.Clients
                            .FirstOrDefaultAsync(c => c.RegistrationNumber == registrationNumber);

                        if (existingClient != null)
                        {
                            if (existingClient.IsActive)
                            {
                                warnings.Add($"Rand {row}: Clientul {registrationNumber} exista deja si este activ - ignorat");
                                continue;
                            }
                            else
                            {
                                // Reactivate inactive client
                                existingClient.ValidityType = validityType;
                                existingClient.ExpiryDate = expiryDate;
                                existingClient.PhoneNumber = phoneNumber;
                                existingClient.IsActive = true;
                                existingClient.CreatedAt = DateTime.Now;
                                existingClient.CreatedByUserId = currentUserId;
                                
                                _context.Update(existingClient);
                                successCount++;
                            }
                        }
                        else
                        {
                            // Create new client
                            var client = new Client
                            {
                                RegistrationNumber = registrationNumber,
                                ValidityType = validityType,
                                ExpiryDate = expiryDate,
                                PhoneNumber = phoneNumber,
                                CreatedByUserId = currentUserId
                            };

                            _context.Add(client);
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        warnings.Add($"Rand {row}: Eroare la procesare - {ex.Message}");
                        errorCount++;
                    }
                }

                if (successCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"Import finalizat: {successCount} clienti procesati cu succes";
                }
                
                if (errorCount > 0)
                {
                    var firstWarnings = warnings.Take(3).ToList();
                    TempData["ErrorMessage"] = $"Avertismente: {errorCount} randuri cu probleme. Primele: {string.Join("; ", firstWarnings)}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Eroare la importul fisierului: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // Validari helper
        private bool IsValidRegistrationNumber(string registrationNumber)
        {
            if (string.IsNullOrEmpty(registrationNumber)) return false;
            
            // Format romanesc: 2-3 litere + 2-3 cifre + 3 litere (ex: B123ABC, CJ45DEF)
            var pattern = @"^[A-Z]{1,3}\d{2,3}[A-Z]{3}$";
            return Regex.IsMatch(registrationNumber, pattern);
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return null;
            
            // Elimina spatii, cratima, puncte
            phoneNumber = Regex.Replace(phoneNumber, @"[\s\-\.]", "");
            
            // Converteste +4 la 0
            if (phoneNumber.StartsWith("+40"))
            {
                phoneNumber = "0" + phoneNumber.Substring(3);
            }
            else if (phoneNumber.StartsWith("40") && phoneNumber.Length == 12)
            {
                phoneNumber = "0" + phoneNumber.Substring(2);
            }
            
            return phoneNumber;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return false;
            
            // Format romanesc: 07XXXXXXXX (10 cifre)
            var pattern = @"^07\d{8}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new ClientDetailsViewModel
            {
                Id = client.Id,
                RegistrationNumber = client.RegistrationNumber,
                ValidityType = client.ValidityType,
                ExpiryDate = client.ExpiryDate,
                PhoneNumber = client.PhoneNumber,
                CreatedAt = client.CreatedAt,
                IsActive = client.IsActive,
                CreatedByUserName = client.CreatedByUser?.UserName
            };

            return View(viewModel);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            var viewModel = new ClientCreateViewModel
            {
                ValidityType = ValidityType.Manual // Default: Manual
            };
            return View(viewModel);
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Normalizeaza numarul de inmatriculare
                    var normalizedRegistrationNumber = viewModel.RegistrationNumber.ToUpper().Trim();

                    // Check if registration number already exists (including inactive clients)
                    var existingClient = await _context.Clients
                        .FirstOrDefaultAsync(c => c.RegistrationNumber == normalizedRegistrationNumber);

                    if (existingClient != null)
                    {
                        if (existingClient.IsActive)
                        {
                            // Client activ cu acelasi numar
                            ModelState.AddModelError(nameof(viewModel.RegistrationNumber), 
                                "Un client activ cu acest numar de inmatriculare exista deja.");
                            return View(viewModel);
                        }
                        else
                        {
                            // Client inactiv - reactiveaza-l cu datele noi
                            existingClient.ValidityType = viewModel.ValidityType;
                            existingClient.ExpiryDate = CalculateExpiryDate(viewModel);
                            existingClient.PhoneNumber = viewModel.PhoneNumber?.Trim();
                            existingClient.IsActive = true;
                            existingClient.CreatedAt = DateTime.Now;
                            existingClient.CreatedByUserId = _userManager.GetUserId(User);

                            _context.Update(existingClient);
                            await _context.SaveChangesAsync();

                            TempData["SuccessMessage"] = $"Clientul {normalizedRegistrationNumber} a fost reactivat cu succes!";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // Nu exista - creaza client nou
                    var client = new Client
                    {
                        RegistrationNumber = normalizedRegistrationNumber,
                        ValidityType = viewModel.ValidityType,
                        ExpiryDate = CalculateExpiryDate(viewModel),
                        PhoneNumber = viewModel.PhoneNumber?.Trim(),
                        CreatedByUserId = _userManager.GetUserId(User)
                    };

                    _context.Add(client);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Clientul a fost adaugat cu succes!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && 
                                                   sqliteEx.SqliteErrorCode == 19) // UNIQUE constraint failed
                {
                    // Fallback in caz ca validarea de mai sus nu a prins duplicatul
                    ModelState.AddModelError(nameof(viewModel.RegistrationNumber), 
                        "Un client cu acest numar de inmatriculare exista deja in sistem.");
                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    // Log the exception (in productie ai avea un logger aici)
                    ModelState.AddModelError("", "A aparut o eroare la salvarea datelor. Te rugam sa incerci din nou.");
                    return View(viewModel);
                }
            }
            return View(viewModel);
        }

        // Metoda helper pentru calcularea datei de expirare
        private DateTime CalculateExpiryDate(ClientCreateViewModel viewModel)
        {
            if (viewModel.ValidityType == ValidityType.Manual)
            {
                return viewModel.ExpiryDate;
            }

            var today = DateTime.Today;
            var months = (int)viewModel.ValidityType;
            return today.AddMonths(months);
        }

        // Metoda helper pentru calcularea datei de expirare (Edit)
        private DateTime CalculateExpiryDate(ClientEditViewModel viewModel)
        {
            if (viewModel.ValidityType == ValidityType.Manual)
            {
                return viewModel.ExpiryDate;
            }

            var today = DateTime.Today;
            var months = (int)viewModel.ValidityType;
            return today.AddMonths(months);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var viewModel = new ClientEditViewModel
            {
                Id = client.Id,
                RegistrationNumber = client.RegistrationNumber,
                ValidityType = client.ValidityType,
                ExpiryDate = client.ExpiryDate,
                PhoneNumber = client.PhoneNumber,
                IsActive = client.IsActive
            };

            return View(viewModel);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var client = await _context.Clients.FindAsync(id);
                    if (client == null)
                    {
                        return NotFound();
                    }

                    // Normalizeaza numarul de inmatriculare
                    var normalizedRegistrationNumber = viewModel.RegistrationNumber.ToUpper().Trim();

                    // Check if registration number already exists for another client
                    var existingClient = await _context.Clients
                        .FirstOrDefaultAsync(c => c.RegistrationNumber == normalizedRegistrationNumber && 
                                                 c.Id != id);

                    if (existingClient != null)
                    {
                        ModelState.AddModelError(nameof(viewModel.RegistrationNumber), 
                            "Un alt client cu acest numar de inmatriculare exista deja.");
                        return View(viewModel);
                    }

                    // Pentru modurile automatice, calculeaza data expirarii pe server
                    DateTime expiryDate = viewModel.ExpiryDate;
                    if (viewModel.ValidityType != ValidityType.Manual)
                    {
                        var today = DateTime.Today;
                        var months = (int)viewModel.ValidityType;
                        expiryDate = today.AddMonths(months);
                    }

                    client.RegistrationNumber = normalizedRegistrationNumber;
                    client.ValidityType = viewModel.ValidityType;
                    client.ExpiryDate = expiryDate;
                    client.PhoneNumber = viewModel.PhoneNumber?.Trim();
                    client.IsActive = viewModel.IsActive;

                    _context.Update(client);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Clientul a fost actualizat cu succes!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && 
                                                   sqliteEx.SqliteErrorCode == 19) // UNIQUE constraint failed
                {
                    ModelState.AddModelError(nameof(viewModel.RegistrationNumber), 
                        "Un alt client cu acest numar de inmatriculare exista deja in sistem.");
                    return View(viewModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "A aparut o eroare la actualizarea datelor. Te rugam sa incerci din nou.");
                    return View(viewModel);
                }
            }
            return View(viewModel);
        }

        // POST: Clients/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                // Soft delete - mark as inactive instead of removing
                client.IsActive = false;
                _context.Update(client);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Clientul a fost dezactivat cu succes!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
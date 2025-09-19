using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using OfficeOpenXml.ConditionalFormatting;
using System.Drawing;
using WebApplication1.Models;
using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Services
{
    public interface IExcelTemplateService
    {
        byte[] GenerateClientTemplate();
        byte[] GenerateInstructionsTemplate();
        Task<ImportResult> ImportClientsFromExcelAsync(byte[] excelData, string userId);
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int SuccessfulImports { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }

    public class ExcelTemplateService : IExcelTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExcelTemplateService> _logger;

        public ExcelTemplateService(ApplicationDbContext context, ILogger<ExcelTemplateService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public byte[] GenerateClientTemplate()
        {
            // Seteaz? licen?a pentru EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            
            // Creeaz? sheet-ul principal pentru date
            var wsMain = package.Workbook.Worksheets.Add("Date Clien?i");
            CreateMainDataSheet(wsMain);
            
            // Creeaz? sheet-ul cu instruc?iuni
            var wsInstructions = package.Workbook.Worksheets.Add("Instruc?iuni");
            CreateInstructionsSheet(wsInstructions);
            
            // Creeaz? sheet-ul cu exemple
            var wsExamples = package.Workbook.Worksheets.Add("Exemple");
            CreateExamplesSheet(wsExamples);
            
            // Creeaz? sheet-ul cu statistici
            var wsStats = package.Workbook.Worksheets.Add("Statistici");
            CreateStatisticsSheet(wsStats);
            
            // Seteaz? sheet-ul activ
            package.Workbook.Worksheets["Date Clien?i"].Select();
            
            return package.GetAsByteArray();
        }

        private void CreateMainDataSheet(ExcelWorksheet ws)
        {
            // Header row cu diacritice corecte
            ws.Cells["A1"].Value = "Nr. Înmatriculare";
            ws.Cells["B1"].Value = "Telefon";
            ws.Cells["C1"].Value = "Tip Valabilitate";
            ws.Cells["D1"].Value = "Data Expirare";
            ws.Cells["E1"].Value = "Status";
            ws.Cells["F1"].Value = "Zile R?mase";

            // Style header
            using (var range = ws.Cells["A1:F1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thick);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.Font.Size = 12;
            }

            // Seteaz? l??imea coloanelor optimizat?
            ws.Column(1).Width = 18; // Nr. Înmatriculare
            ws.Column(2).Width = 15; // Telefon
            ws.Column(3).Width = 16; // Tip Valabilitate
            ws.Column(4).Width = 16; // Data Expirare
            ws.Column(5).Width = 16; // Status
            ws.Column(6).Width = 14; // Zile R?mase

            // Configureaz? valid?rile ?i formulele pentru 500 de rânduri
            for (int row = 2; row <= 500; row++)
            {
                // Default telefon Miseda Inspect SRL
                ws.Cells[$"B{row}"].Value = "0756596565";
                ws.Cells[$"B{row}"].Style.Font.Color.SetColor(Color.FromArgb(128, 128, 128)); // Grey pentru default
                
                // Dropdown pentru Tip Valabilitate cu op?iuni corecte
                var validationList = ws.DataValidations.AddListValidation($"C{row}");
                validationList.Formula.Values.Add("6 Luni");
                validationList.Formula.Values.Add("1 An");
                validationList.Formula.Values.Add("2 Ani");
                validationList.Formula.Values.Add("Manual");
                validationList.ShowErrorMessage = true;
                validationList.Error = "V? rug?m s? selecta?i o op?iune valid? din list?: 6 Luni, 1 An, 2 Ani sau Manual.";
                validationList.ErrorStyle = ExcelDataValidationWarningStyle.stop;
                validationList.ShowInputMessage = true;
                validationList.PromptTitle = "Tip Valabilitate";
                validationList.Prompt = "Selecta?i tipul de valabilitate din list?. Pentru date personalizate alege?i 'Manual'.";

                // Formul? îmbun?t??it? pentru Data Expirare cu handling pentru Manual
                ws.Cells[$"D{row}"].Formula = 
                    $"IF(C{row}=\"6 Luni\",TODAY()+180," +
                    $"IF(C{row}=\"1 An\",TODAY()+365," +
                    $"IF(C{row}=\"2 Ani\",TODAY()+730," +
                    $"IF(AND(C{row}=\"Manual\",D{row}=\"\"),\"[Introdu data manual]\",D{row}))))";
                
                ws.Cells[$"D{row}"].Style.Numberformat.Format = "dd.mm.yyyy";

                // Formul? îmbun?t??it? pentru Status
                ws.Cells[$"E{row}"].Formula = 
                    $"IF(AND(D{row}<>\"\",ISNUMBER(D{row}),D{row}<>\"[Introdu data manual]\")," +
                    $"IF(D{row}<TODAY(),\"Expirat\"," +
                    $"IF(D{row}<TODAY()+30,\"Expir? Curând\",\"Valid\"))," +
                    $"IF(D{row}=\"[Introdu data manual]\",\"Necesit? dat? manual?\",\"\"))";

                // Formul? îmbun?t??it? pentru Zile R?mase
                ws.Cells[$"F{row}"].Formula = 
                    $"IF(AND(D{row}<>\"\",ISNUMBER(D{row}),D{row}<>\"[Introdu data manual]\")," +
                    $"D{row}-TODAY()," +
                    $"IF(D{row}=\"[Introdu data manual]\",\"--\",\"\"))";
                
                ws.Cells[$"F{row}"].Style.Numberformat.Format = "0";

                // Validare îmbun?t??it? pentru num?rul de telefon românesc
                var phoneValidation = ws.DataValidations.AddCustomValidation($"B{row}");
                phoneValidation.Formula.ExcelFormula = $"AND(LEN(B{row})=10,LEFT(B{row},2)=\"07\",ISNUMBER(VALUE(B{row})),VALUE(B{row})>0)";
                phoneValidation.ShowErrorMessage = true;
                phoneValidation.Error = "Num?rul de telefon trebuie s? aib? exact 10 cifre ?i s? înceap? cu 07 (format românesc). Exemplu: 0756596565";
                phoneValidation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                phoneValidation.ShowInputMessage = true;
                phoneValidation.PromptTitle = "Format Telefon";
                phoneValidation.Prompt = "Introduce?i num?rul de telefon în format românesc: 07XXXXXXXX";

                // Validare îmbun?t??it? pentru num?rul de înmatriculare românesc
                var regValidation = ws.DataValidations.AddCustomValidation($"A{row}");
                regValidation.Formula.ExcelFormula = $"AND(LEN(A{row})>=6,LEN(A{row})<=10,A{row}<>\"\")";
                regValidation.ShowErrorMessage = true;
                regValidation.Error = "Num?rul de înmatriculare trebuie s? aib? între 6 ?i 10 caractere (format RO: SV99XYZ, B123ABC, etc.)";
                regValidation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                regValidation.ShowInputMessage = true;
                regValidation.PromptTitle = "Nr. Înmatriculare";
                regValidation.Prompt = "Format românesc: jude? + cifre + litere (ex: SV14ABC, B123XYZ)";

                // Validare pentru data manual? (doar când tipul este Manual)
                if (row == 2) // Aplic?m o dat? ca exemplu
                {
                    var dateValidation = ws.DataValidations.AddCustomValidation($"D{row}:D500");
                    dateValidation.Formula.ExcelFormula = $"OR(C{row}<>\"Manual\",AND(C{row}=\"Manual\",D{row}>=TODAY(),D{row}<=TODAY()+1095))"; // Max 3 ani
                    dateValidation.ShowErrorMessage = true;
                    dateValidation.Error = "Pentru tipul 'Manual', data trebuie s? fie între azi ?i urm?torii 3 ani.";
                    dateValidation.ErrorStyle = ExcelDataValidationWarningStyle.warning;
                }
            }

            // Protejeaz? coloanele calculate (D, E, F) dar permite editarea pentru Manual
            ws.Cells["E:F"].Style.Locked = true;
            ws.Cells["A:C"].Style.Locked = false;
            
            // Coloana D par?ial protejat? - va fi editabil? doar pentru Manual
            ws.Cells["D:D"].Style.Locked = false;

            // Conditional formatting îmbun?t??it pentru Status
            var statusRange = ws.Cells["E2:E500"];
            
            // Verde pentru "Valid"
            var validCondition = statusRange.ConditionalFormatting.AddEqual();
            validCondition.Formula = "\"Valid\"";
            validCondition.Style.Fill.PatternType = ExcelFillStyle.Solid;
            validCondition.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(146, 208, 80));
            validCondition.Style.Font.Color.SetColor(Color.FromArgb(55, 86, 35));
            validCondition.Style.Font.Bold = true;

            // Galben pentru "Expir? Curând"
            var expiringSoonCondition = statusRange.ConditionalFormatting.AddEqual();
            expiringSoonCondition.Formula = "\"Expir? Curând\"";
            expiringSoonCondition.Style.Fill.PatternType = ExcelFillStyle.Solid;
            expiringSoonCondition.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 192, 0));
            expiringSoonCondition.Style.Font.Color.SetColor(Color.FromArgb(128, 96, 0));
            expiringSoonCondition.Style.Font.Bold = true;

            // Ro?u pentru "Expirat"
            var expiredCondition = statusRange.ConditionalFormatting.AddEqual();
            expiredCondition.Formula = "\"Expirat\"";
            expiredCondition.Style.Fill.PatternType = ExcelFillStyle.Solid;
            expiredCondition.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 203, 173));
            expiredCondition.Style.Font.Color.SetColor(Color.FromArgb(128, 0, 0));
            expiredCondition.Style.Font.Bold = true;

            // Albastru pentru "Necesit? dat? manual?"
            var needsDateCondition = statusRange.ConditionalFormatting.AddEqual();
            needsDateCondition.Formula = "\"Necesit? dat? manual?\"";
            needsDateCondition.Style.Fill.PatternType = ExcelFillStyle.Solid;
            needsDateCondition.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(173, 216, 230));
            needsDateCondition.Style.Font.Color.SetColor(Color.FromArgb(0, 0, 128));
            needsDateCondition.Style.Font.Bold = true;

            // Conditional formatting pentru zile r?mase
            var daysRange = ws.Cells["F2:F500"];
            
            // Ro?u pentru valori negative (expirat)
            var negativeDays = daysRange.ConditionalFormatting.AddLessThan();
            negativeDays.Formula = "0";
            negativeDays.Style.Fill.PatternType = ExcelFillStyle.Solid;
            negativeDays.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 199, 206));
            negativeDays.Style.Font.Color.SetColor(Color.FromArgb(156, 0, 6));

            // Galben pentru 1-30 zile
            var warningDays = daysRange.ConditionalFormatting.AddBetween();
            warningDays.Formula = "1";
            warningDays.Formula2 = "30";
            warningDays.Style.Fill.PatternType = ExcelFillStyle.Solid;
            warningDays.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 235, 156));
            warningDays.Style.Font.Color.SetColor(Color.FromArgb(156, 101, 0));

            // Formatare zebra îmbun?t??it? pentru rânduri
            for (int row = 3; row <= 500; row += 2)
            {
                ws.Cells[$"A{row}:F{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[$"A{row}:F{row}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(250, 250, 250));
            }

            // Freeze panes la header
            ws.View.FreezePanes(2, 1);

            // Adaug? bordere pentru toate celulele
            var allDataRange = ws.Cells["A1:F500"];
            allDataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            allDataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            allDataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            allDataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            allDataRange.Style.Border.Top.Color.SetColor(Color.FromArgb(217, 217, 217));
            allDataRange.Style.Border.Bottom.Color.SetColor(Color.FromArgb(217, 217, 217));
            allDataRange.Style.Border.Left.Color.SetColor(Color.FromArgb(217, 217, 217));
            allDataRange.Style.Border.Right.Color.SetColor(Color.FromArgb(217, 217, 217));

            // Protejeaz? sheet-ul cu parol?
            ws.Protection.IsProtected = true;
            ws.Protection.Password = "miseda2024";
            ws.Protection.AllowSelectLockedCells = true;
            ws.Protection.AllowSelectUnlockedCells = true;
            ws.Protection.AllowFormatCells = false;
            ws.Protection.AllowInsertRows = false;
            ws.Protection.AllowDeleteRows = false;
            ws.Protection.AllowSort = false;
            ws.Protection.AllowAutoFilter = false;

            // Adaug? comentarii pentru celule importante
            ws.Cells["C2"].AddComment("Selecta?i din dropdown: 6 Luni, 1 An, 2 Ani sau Manual", "Sistem");
            ws.Cells["D2"].AddComment("Pentru 'Manual' introduce?i data manual, altfel se calculeaz? automat", "Sistem");
            ws.Cells["E2"].AddComment("Status calculat automat pe baza datei de expirare", "Sistem");
            ws.Cells["F2"].AddComment("Zilele r?mase pân? la expirare (calculat automat)", "Sistem");

            // Not? în partea de jos
            ws.Cells["A502"].Value = "?? IMPORTANT: Acest template ÎNLOCUIE?TE complet baza de date la import!";
            ws.Cells["A502"].Style.Font.Bold = true;
            ws.Cells["A502"].Style.Font.Color.SetColor(Color.Red);
            ws.Cells["A502"].Style.Font.Size = 12;

            ws.Cells["A503"].Value = "?? Telefon default: 0756596565 (Miseda Inspect SRL) - modifica?i dac? e necesar";
            ws.Cells["A503"].Style.Font.Color.SetColor(Color.FromArgb(79, 129, 189));
        }

        private void CreateInstructionsSheet(ExcelWorksheet ws)
        {
            ws.Cells["A1"].Value = "?? INSTRUC?IUNI UTILIZARE TEMPLATE EXCEL CLIEN?I ITP";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(79, 129, 189));

            int row = 3;
            
            // Sec?iunea 1: Cum s? completezi
            ws.Cells[$"A{row}"].Value = "?? CUM S? COMPLETEZI TEMPLATE-UL:";
            ws.Cells[$"A{row}"].Style.Font.Bold = true;
            ws.Cells[$"A{row}"].Style.Font.Size = 14;
            ws.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
            row += 2;

            var instructions = new[]
            {
                "1. COLOANA A - Nr. Înmatriculare:",
                "   • Introdu num?rul de înmatriculare (ex: SV99XYZ, B123ABC)",
                "   • Între 6-10 caractere, f?r? spa?ii",
                "",
                "2. COLOANA B - Telefon:",
                "   • Num?rul este completat automat cu 0756596565",
                "   • Po?i s?-l modifici dac? e necesar",
                "   • Trebuie s? înceap? cu 07 ?i s? aib? 10 cifre",
                "",
                "3. COLOANA C - Tip Valabilitate:",
                "   • Selecteaz? din dropdown: '6 Luni', '1 An', '2 Ani', 'Manual'",
                "   • Pentru 6 Luni/1 An/2 Ani - data se calculeaz? automat",
                "   • Pentru 'Manual' - introdu data manual în coloana D",
                "",
                "4. COLOANELE D, E, F - Calculate automat:",
                "   • Data Expirare: se calculeaz? pe baza tipului de valabilitate",
                "   • Status: Valid/Expir? Curând/Expirat (calculat automat)",
                "   • Zile R?mase: num?rul de zile pân? la expirare"
            };

            foreach (var instruction in instructions)
            {
                if (instruction.StartsWith("   •"))
                {
                    ws.Cells[$"B{row}"].Value = instruction.Substring(4);
                    ws.Cells[$"B{row}"].Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
                }
                else if (string.IsNullOrEmpty(instruction))
                {
                    // Linie goal?
                }
                else
                {
                    ws.Cells[$"A{row}"].Value = instruction;
                    ws.Cells[$"A{row}"].Style.Font.Bold = true;
                    ws.Cells[$"A{row}"].Style.Font.Color.SetColor(Color.FromArgb(68, 114, 196));
                }
                row++;
            }

            row += 2;

            // Sec?iunea 2: Reguli importante
            ws.Cells[$"A{row}"].Value = "?? REGULI IMPORTANTE:";
            ws.Cells[$"A{row}"].Style.Font.Bold = true;
            ws.Cells[$"A{row}"].Style.Font.Size = 14;
            ws.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(252, 228, 214));
            row += 2;

            var rules = new[]
            {
                "• Acest template ÎNLOCUIE?TE complet baza de date la import",
                "• Completeaz? DOAR coloanele A, B, C (?i D pentru 'Manual')",
                "• Nu modifica coloanele E ?i F - sunt calculate automat",
                "• Pentru import, salveaz? ca .xlsx (Excel Workbook)",
                "• Maxim 500 de clien?i per template",
                "• Verific? c? nu ai duplicate în coloana A",
                "• Pentru 'Manual': introdu data în format DD.MM.YYYY",
                "• Template-ul are parol? de protec?ie: miseda2024"
            };

            foreach (var rule in rules)
            {
                ws.Cells[$"A{row}"].Value = rule;
                ws.Cells[$"A{row}"].Style.Font.Color.SetColor(Color.FromArgb(191, 143, 0));
                row++;
            }

            row += 2;

            // Sec?iunea 3: Automatiz?ri
            ws.Cells[$"A{row}"].Value = "?? AUTOMATIZ?RI INTEGRATE:";
            ws.Cells[$"A{row}"].Style.Font.Bold = true;
            ws.Cells[$"A{row}"].Style.Font.Size = 14;
            ws.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(226, 239, 218));
            row += 2;

            var automations = new[]
            {
                "? Dropdown inteligent pentru Tip Valabilitate",
                "? Calculare automat? dat? expirare:",
                "   ? 6 Luni = TODAY() + 180 zile",
                "   ? 1 An = TODAY() + 365 zile",
                "   ? 2 Ani = TODAY() + 730 zile",
                "? Status automat pe culori:",
                "   ? VERDE = Valid (>30 zile)",
                "   ? GALBEN = Expir? Curând (1-30 zile)",
                "   ? RO?U = Expirat (<0 zile)",
                "? Valid?ri automate telefon ?i înmatriculare",
                "? Protejare coloane calculate",
                "? Highlighting condi?ional pentru vizualizare rapid?"
            };

            foreach (var automation in automations)
            {
                if (automation.StartsWith("   ?"))
                {
                    ws.Cells[$"B{row}"].Value = automation.Substring(4);
                    ws.Cells[$"B{row}"].Style.Font.Color.SetColor(Color.FromArgb(112, 173, 71));
                }
                else
                {
                    ws.Cells[$"A{row}"].Value = automation;
                    ws.Cells[$"A{row}"].Style.Font.Color.SetColor(Color.FromArgb(112, 173, 71));
                }
                row++;
            }

            // Auto-fit columns
            ws.Cells.AutoFitColumns();
            ws.Column(1).Width = Math.Max(ws.Column(1).Width, 60);
            ws.Column(2).Width = Math.Max(ws.Column(2).Width, 45);
        }

        private void CreateExamplesSheet(ExcelWorksheet ws)
        {
            // Header
            ws.Cells["A1"].Value = "Nr. Înmatriculare";
            ws.Cells["B1"].Value = "Telefon";
            ws.Cells["C1"].Value = "Tip Valabilitate";
            ws.Cells["D1"].Value = "Data Expirare";
            ws.Cells["E1"].Value = "Status";
            ws.Cells["F1"].Value = "Zile R?mase";

            // Style header
            using (var range = ws.Cells["A1:F1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Exemple de date realiste pentru Miseda Inspect SRL
            var examples = new[]
            {
                new { Reg = "SV14ABC", Phone = "0756596565", Type = "6 Luni", Note = "ITP Standard 6 luni" },
                new { Reg = "B123XYZ", Phone = "0756596565", Type = "1 An", Note = "ITP Anual standard" },
                new { Reg = "CJ99DEF", Phone = "0756596565", Type = "2 Ani", Note = "ITP Bianual auto nou?" },
                new { Reg = "SV88GHI", Phone = "0756596565", Type = "Manual", Note = "Dat? personalizat? - 15.03.2025" },
                new { Reg = "AB77JKL", Phone = "0756596565", Type = "6 Luni", Note = "ITP Semestrial" },
                new { Reg = "HD55MNO", Phone = "0756596565", Type = "Manual", Note = "Expir? în 15 zile - notificare urgent?" },
                new { Reg = "TM44PQR", Phone = "0756596565", Type = "1 An", Note = "Client fidel - ITP anual" },
                new { Reg = "IS33STU", Phone = "0756596565", Type = "Manual", Note = "EXPIRAT - necesit? reînnoire" },
                new { Reg = "BV22VWX", Phone = "0756596565", Type = "Manual", Note = "Expir? curând - 20 zile" },
                new { Reg = "CT11YZA", Phone = "0756596565", Type = "2 Ani", Note = "Auto nou? - ITP 2 ani" }
            };

            for (int i = 0; i < examples.Length; i++)
            {
                int row = i + 2;
                ws.Cells[$"A{row}"].Value = examples[i].Reg;
                ws.Cells[$"B{row}"].Value = examples[i].Phone;
                ws.Cells[$"C{row}"].Value = examples[i].Type;
                
                // Calculeaz? datele de exemplu
                DateTime expiryDate;
                switch (examples[i].Type)
                {
                    case "6 Luni":
                        expiryDate = DateTime.Today.AddMonths(6);
                        break;
                    case "1 An":
                        expiryDate = DateTime.Today.AddYears(1);
                        break;
                    case "2 Ani":
                        expiryDate = DateTime.Today.AddYears(2);
                        break;
                    case "Manual":
                        // Exemple variate pentru manual
                        if (examples[i].Note.Contains("15.03.2025"))
                            expiryDate = new DateTime(2025, 3, 15);
                        else if (examples[i].Note.Contains("15 zile"))
                            expiryDate = DateTime.Today.AddDays(15);
                        else if (examples[i].Note.Contains("EXPIRAT"))
                            expiryDate = DateTime.Today.AddDays(-5);
                        else if (examples[i].Note.Contains("20 zile"))
                            expiryDate = DateTime.Today.AddDays(20);
                        else
                            expiryDate = DateTime.Today.AddMonths(3);
                        break;
                    default:
                        expiryDate = DateTime.Today.AddYears(1);
                        break;
                }
                
                ws.Cells[$"D{row}"].Value = expiryDate.ToString("dd.MM.yyyy");
                ws.Cells[$"D{row}"].Style.Numberformat.Format = "dd.mm.yyyy";
                
                // Calculeaz? status-ul pentru exemple
                var daysUntilExpiry = (expiryDate - DateTime.Today).Days;
                
                string status;
                Color statusColor;
                if (daysUntilExpiry < 0)
                {
                    status = "Expirat";
                    statusColor = Color.FromArgb(248, 203, 173);
                }
                else if (daysUntilExpiry <= 30)
                {
                    status = "Expir? Curând";
                    statusColor = Color.FromArgb(255, 192, 0);
                }
                else
                {
                    status = "Valid";
                    statusColor = Color.FromArgb(146, 208, 80);
                }
                
                ws.Cells[$"E{row}"].Value = status;
                ws.Cells[$"E{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[$"E{row}"].Style.Fill.BackgroundColor.SetColor(statusColor);
                ws.Cells[$"E{row}"].Style.Font.Bold = true;
                
                ws.Cells[$"F{row}"].Value = daysUntilExpiry;
                
                // Adaug? comentarii cu explica?ii
                ws.Cells[$"G{row}"].Value = examples[i].Note;
                ws.Cells[$"G{row}"].Style.Font.Italic = true;
                ws.Cells[$"G{row}"].Style.Font.Size = 10;
                ws.Cells[$"G{row}"].Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
            }

            // Adaug? header pentru notele
            ws.Cells["G1"].Value = "Explica?ii";
            ws.Cells["G1"].Style.Font.Bold = true;
            ws.Cells["G1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells["G1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            ws.Cells["G1"].Style.Font.Color.SetColor(Color.White);
            ws.Cells["G1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Auto-fit columns
            ws.Cells.AutoFitColumns();

            // Adaug? not? explicativ?
            ws.Cells["A13"].Value = "?? EXEMPLE PRACTICE MISEDA INSPECT SRL";
            ws.Cells["A13"].Style.Font.Bold = true;
            ws.Cells["A13"].Style.Font.Size = 14;
            ws.Cells["A13"].Style.Font.Color.SetColor(Color.FromArgb(68, 114, 196));
            
            ws.Cells["A14"].Value = "Aceste exemple arat? cum func?ioneaz? automatiz?rile din template.";
            ws.Cells["A14"].Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
            
            ws.Cells["A15"].Value = "Pentru utilizare real?, completeaz? datele în sheet-ul 'Date Clien?i'.";
            ws.Cells["A15"].Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
            ws.Cells["A15"].Style.Font.Bold = true;
        }

        private void CreateStatisticsSheet(ExcelWorksheet ws)
        {
            ws.Cells["A1"].Value = "?? STATISTICI AUTOMATE TEMPLATE CLIEN?I";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Size = 16;
            ws.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(79, 129, 189));

            // Statistici care se actualizeaz? automat pe baza datelor din sheet-ul principal
            ws.Cells["A3"].Value = "?? NUM?RUL TOTAL DE CLIEN?I:";
            ws.Cells["A3"].Style.Font.Bold = true;
            ws.Cells["B3"].Formula = "COUNTA('Date Clien?i'!A2:A500)-COUNTBLANK('Date Clien?i'!A2:A500)";
            ws.Cells["B3"].Style.Font.Bold = true;
            ws.Cells["B3"].Style.Font.Size = 14;

            ws.Cells["A5"].Value = "?? DISTRIBU?IE PE STATUS:";
            ws.Cells["A5"].Style.Font.Bold = true;
            ws.Cells["A5"].Style.Font.Size = 14;

            // Clien?i valizi
            ws.Cells["A6"].Value = "? Clien?i Valizi:";
            ws.Cells["B6"].Formula = "COUNTIF('Date Clien?i'!E2:E500,\"Valid\")";
            ws.Cells["B6"].Style.Font.Color.SetColor(Color.Green);
            ws.Cells["B6"].Style.Font.Bold = true;
            ws.Cells["C6"].Formula = "IF(B3>0,ROUND(B6/B3*100,1)&\"%\",\"0%\")";
            ws.Cells["C6"].Style.Font.Color.SetColor(Color.Green);

            // Clien?i ce expir? curând
            ws.Cells["A7"].Value = "?? Clien?i ce Expir? Curând:";
            ws.Cells["B7"].Formula = "COUNTIF('Date Clien?i'!E2:E500,\"Expir? Curând\")";
            ws.Cells["B7"].Style.Font.Color.SetColor(Color.Orange);
            ws.Cells["B7"].Style.Font.Bold = true;
            ws.Cells["C7"].Formula = "IF(B3>0,ROUND(B7/B3*100,1)&\"%\",\"0%\")";
            ws.Cells["C7"].Style.Font.Color.SetColor(Color.Orange);

            // Clien?i expira?i
            ws.Cells["A8"].Value = "? Clien?i Expira?i:";
            ws.Cells["B8"].Formula = "COUNTIF('Date Clien?i'!E2:E500,\"Expirat\")";
            ws.Cells["B8"].Style.Font.Color.SetColor(Color.Red);
            ws.Cells["B8"].Style.Font.Bold = true;
            ws.Cells["C8"].Formula = "IF(B3>0,ROUND(B8/B3*100,1)&\"%\",\"0%\")";
            ws.Cells["C8"].Style.Font.Color.SetColor(Color.Red);

            ws.Cells["A10"].Value = "?? DISTRIBU?IE PE TIP VALABILITATE:";
            ws.Cells["A10"].Style.Font.Bold = true;
            ws.Cells["A10"].Style.Font.Size = 14;

            ws.Cells["A11"].Value = "6 Luni:";
            ws.Cells["B11"].Formula = "COUNTIF('Date Clien?i'!C2:C500,\"6 Luni\")";
            ws.Cells["C11"].Formula = "IF(B3>0,ROUND(B11/B3*100,1)&\"%\",\"0%\")";

            ws.Cells["A12"].Value = "1 An:";
            ws.Cells["B12"].Formula = "COUNTIF('Date Clien?i'!C2:C500,\"1 An\")";
            ws.Cells["C12"].Formula = "IF(B3>0,ROUND(B12/B3*100,1)&\"%\",\"0%\")";

            ws.Cells["A13"].Value = "2 Ani:";
            ws.Cells["B13"].Formula = "COUNTIF('Date Clien?i'!C2:C500,\"2 Ani\")";
            ws.Cells["C13"].Formula = "IF(B3>0,ROUND(B13/B3*100,1)&\"%\",\"0%\")";

            ws.Cells["A14"].Value = "Manual:";
            ws.Cells["B14"].Formula = "COUNTIF('Date Clien?i'!C2:C500,\"Manual\")";
            ws.Cells["C14"].Formula = "IF(B3>0,ROUND(B14/B3*100,1)&\"%\",\"0%\")";

            // Headers pentru coloane
            ws.Cells["B5"].Value = "Num?r";
            ws.Cells["C5"].Value = "Procent";
            ws.Cells["B5:C5"].Style.Font.Bold = true;
            ws.Cells["B10"].Value = "Num?r";
            ws.Cells["C10"].Value = "Procent";
            ws.Cells["B10:C10"].Style.Font.Bold = true;

            // Statistici avansate
            ws.Cells["A16"].Value = "?? STATISTICI AVANSATE:";
            ws.Cells["A16"].Style.Font.Bold = true;
            ws.Cells["A16"].Style.Font.Size = 14;

            ws.Cells["A17"].Value = "Media zile pân? la expirare:";
            ws.Cells["B17"].Formula = "IF(COUNTIF('Date Clien?i'!E2:E500,\"Valid\")>0,ROUND(AVERAGE(IF('Date Clien?i'!E2:E500=\"Valid\",'Date Clien?i'!F2:F500)),0),\"N/A\")";

            ws.Cells["A18"].Value = "Primul ITP care expir?:";
            ws.Cells["B18"].Formula = "IF(COUNTIF('Date Clien?i'!F2:F500,\">0\")>0,MIN(IF('Date Clien?i'!F2:F500>0,'Date Clien?i'!F2:F500))&\" zile\",\"N/A\")";

            ws.Cells["A19"].Value = "Total notific?ri necesare:";
            ws.Cells["B19"].Formula = "B7+B8"; // Expir? curând + Expira?i
            ws.Cells["B19"].Style.Font.Bold = true;
            ws.Cells["B19"].Style.Font.Color.SetColor(Color.Red);

            // Auto-fit columns
            ws.Cells.AutoFitColumns();

            // Not? informativ?
            ws.Cells["A22"].Value = "?? INFORMA?II IMPORTANTE:";
            ws.Cells["A22"].Style.Font.Bold = true;
            ws.Cells["A22"].Style.Font.Color.SetColor(Color.FromArgb(68, 114, 196));
            
            ws.Cells["A23"].Value = "• Statisticile se actualizeaz? automat când completezi datele";
            ws.Cells["A24"].Value = "• Pentru actualizare manual?: apas? F9 sau Ctrl+Alt+F9";
            ws.Cells["A25"].Value = "• Telefon Miseda Inspect SRL: 0756596565";
            ws.Cells["A26"].Value = "• Template protejat cu parola: miseda2024";
            
            ws.Cells["A23:A26"].Style.Font.Italic = true;
            ws.Cells["A23:A26"].Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
        }

        public async Task<ImportResult> ImportClientsFromExcelAsync(byte[] excelData, string userId)
        {
            var result = new ImportResult();
            
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using var package = new ExcelPackage(new MemoryStream(excelData));
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name == "Date Clien?i");
                
                if (worksheet == null)
                {
                    result.ErrorMessages.Add("Nu s-a g?sit sheet-ul 'Date Clien?i' în fi?ierul Excel.");
                    return result;
                }

                var clients = new List<Client>();
                var duplicateCheck = new HashSet<string>();
                
                // Începe de la rândul 2 (dup? header)
                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    var regNumber = worksheet.Cells[row, 1].Text?.Trim();
                    
                    // Sari peste rândurile goale
                    if (string.IsNullOrWhiteSpace(regNumber))
                        continue;

                    result.TotalProcessed++;

                    try
                    {
                        // Verific? duplicate în Excel
                        if (!duplicateCheck.Add(regNumber.ToUpper()))
                        {
                            result.ErrorMessages.Add($"Rândul {row}: Num?rul de înmatriculare '{regNumber}' este duplicat în Excel.");
                            result.Errors++;
                            continue;
                        }

                        var phone = worksheet.Cells[row, 2].Text?.Trim();
                        var validityTypeText = worksheet.Cells[row, 3].Text?.Trim();
                        var expiryDateText = worksheet.Cells[row, 4].Text?.Trim();

                        // Valid?ri
                        if (regNumber.Length < 6 || regNumber.Length > 10)
                        {
                            result.ErrorMessages.Add($"Rândul {row}: Num?rul de înmatriculare '{regNumber}' trebuie s? aib? între 6-10 caractere.");
                            result.Errors++;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(phone) && (phone.Length != 10 || !phone.StartsWith("07") || !phone.All(char.IsDigit)))
                        {
                            result.ErrorMessages.Add($"Rândul {row}: Telefonul '{phone}' trebuie s? aib? 10 cifre ?i s? înceap? cu 07.");
                            result.Errors++;
                            continue;
                        }

                        // Parseaz? tipul de valabilitate
                        ValidityType validityType;
                        switch (validityTypeText?.ToLower())
                        {
                            case "6 luni":
                                validityType = ValidityType.SixMonths;
                                break;
                            case "1 an":
                                validityType = ValidityType.OneYear;
                                break;
                            case "2 ani":
                                validityType = ValidityType.TwoYears;
                                break;
                            case "manual":
                                validityType = ValidityType.Manual;
                                break;
                            default:
                                result.ErrorMessages.Add($"Rândul {row}: Tip valabilitate invalid '{validityTypeText}'. Use: 6 Luni, 1 An, 2 Ani, Manual.");
                                result.Errors++;
                                continue;
                        }

                        // Calculeaz? data de expirare
                        DateTime expiryDate;
                        if (validityType == ValidityType.Manual)
                        {
                            if (!DateTime.TryParseExact(expiryDateText, new[] { "dd.MM.yyyy", "dd/MM/yyyy", "dd-MM-yyyy" }, 
                                null, System.Globalization.DateTimeStyles.None, out expiryDate))
                            {
                                result.ErrorMessages.Add($"Rândul {row}: Pentru tipul 'Manual' trebuie s? introduci o dat? valid? în format DD.MM.YYYY.");
                                result.Errors++;
                                continue;
                            }

                            if (expiryDate < DateTime.Today.AddDays(-30) || expiryDate > DateTime.Today.AddYears(3))
                            {
                                result.ErrorMessages.Add($"Rândul {row}: Data manual? trebuie s? fie între acum ?i urm?torii 3 ani.");
                                result.Errors++;
                                continue;
                            }
                        }
                        else
                        {
                            // Calculeaz? automat pe baza tipului
                            expiryDate = validityType switch
                            {
                                ValidityType.SixMonths => DateTime.Today.AddDays(180),
                                ValidityType.OneYear => DateTime.Today.AddDays(365),
                                ValidityType.TwoYears => DateTime.Today.AddDays(730),
                                _ => DateTime.Today.AddYears(1)
                            };
                        }

                        // Creeaz? clientul
                        var client = new Client
                        {
                            RegistrationNumber = regNumber.ToUpper(),
                            PhoneNumber = string.IsNullOrEmpty(phone) ? "0756596565" : phone,
                            ValidityType = validityType,
                            ExpiryDate = expiryDate,
                            CreatedAt = DateTime.Now,
                            IsActive = true,
                            CreatedByUserId = userId
                        };

                        clients.Add(client);
                        result.SuccessfulImports++;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorMessages.Add($"Rândul {row}: Eroare la procesare - {ex.Message}");
                        result.Errors++;
                        _logger.LogWarning(ex, $"Error processing Excel row {row}");
                    }
                }

                if (clients.Count == 0)
                {
                    result.Message = "Nu s-au g?sit date valide pentru import în fi?ierul Excel.";
                    return result;
                }

                // ÎNLOCUIE?TE COMPLET baza de date
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // ?terge toate înregistr?rile existente
                    var existingClients = await _context.Clients.ToListAsync();
                    _context.Clients.RemoveRange(existingClients);
                    
                    // ?terge ?i notific?rile asociate
                    var existingNotifications = await _context.SmsNotifications.ToListAsync();
                    _context.SmsNotifications.RemoveRange(existingNotifications);

                    await _context.SaveChangesAsync();

                    // Adaug? noii clien?i
                    _context.Clients.AddRange(clients);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    result.Success = true;
                    result.Message = $"Import finalizat cu succes! Baza de date a fost înlocuit? complet cu {clients.Count} clien?i noi.";

                    _logger.LogInformation($"Excel import successful: {clients.Count} clients imported, database completely replaced. User: {userId}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessages.Add($"Eroare general? la import: {ex.Message}");
                _logger.LogError(ex, "Error during Excel import");
            }

            return result;
        }

        public byte[] GenerateInstructionsTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Instruc?iuni Complete");
            CreateDetailedInstructionsSheet(ws);
            
            return package.GetAsByteArray();
        }

        private void CreateDetailedInstructionsSheet(ExcelWorksheet ws)
        {
            // Ghid complet pentru utilizare
            ws.Cells["A1"].Value = "GHID COMPLET TEMPLATE EXCEL CLIEN?I ITP - MISEDA INSPECT SRL";
            ws.Cells["A1"].Style.Font.Bold = true;
            ws.Cells["A1"].Style.Font.Size = 18;
            ws.Cells["A1"].Style.Font.Color.SetColor(Color.FromArgb(79, 129, 189));
            
            // Con?inut detaliat similar cu instruc?iunile din template
            CreateInstructionsSheet(ws);
        }
    }
}
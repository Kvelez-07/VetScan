using ClosedXML.Excel;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using System.Linq;
using System.Threading.Tasks;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class VaccinationHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccinationHistoriesController> _logger;

        public VaccinationHistoriesController(ApplicationDbContext context, ILogger<VaccinationHistoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.Pet.PetName.Contains(searchString) ||
                    v.Vaccine.VaccineName.Contains(searchString) ||
                    (v.Veterinarian.User.FirstName + " " + v.Veterinarian.User.LastName).Contains(searchString) ||
                    v.BatchNumber!.Contains(searchString));
            }

            var vaccinations = await query
                .OrderByDescending(v => v.VaccinationDate)
                .Select(v => new VaccinationHistoryListViewModel
                {
                    VaccinationId = v.VaccinationId,
                    PetName = v.Pet.PetName,
                    VaccineName = v.Vaccine.VaccineName,
                    VeterinarianName = $"{v.Veterinarian.User.FirstName} {v.Veterinarian.User.LastName}",
                    VaccinationDate = v.VaccinationDate,
                    BatchNumber = v.BatchNumber!,
                    NextDueDate = v.NextDueDate
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("HistorialVacunacion");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Mascota";
                worksheet.Cell(currentRow, 2).Value = "Vacuna";
                worksheet.Cell(currentRow, 3).Value = "Veterinario";
                worksheet.Cell(currentRow, 4).Value = "Fecha Vacunación";
                worksheet.Cell(currentRow, 5).Value = "Número de Lote";
                worksheet.Cell(currentRow, 6).Value = "Próxima Dosis";
                worksheet.Cell(currentRow, 7).Value = "Estado";
                worksheet.Cell(currentRow, 8).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 8);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in vaccinations)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.PetName;
                    worksheet.Cell(currentRow, 2).Value = item.VaccineName;
                    worksheet.Cell(currentRow, 3).Value = item.VeterinarianName;
                    worksheet.Cell(currentRow, 4).Value = item.VaccinationDate.ToString("dd/MM/yyyy");
                    worksheet.Cell(currentRow, 5).Value = item.BatchNumber;
                    worksheet.Cell(currentRow, 6).Value = item.NextDueDate?.ToString("dd/MM/yyyy") ?? "N/A";

                    // Determinar estado y color
                    string status;
                    XLColor statusColor;

                    if (item.NextDueDate.HasValue)
                    {
                        status = item.NextDueDate.Value < DateTime.Today ? "Vencida" : "Pendiente";
                        statusColor = item.NextDueDate.Value < DateTime.Today ? XLColor.Red : XLColor.Green;
                    }
                    else
                    {
                        status = "No aplica";
                        statusColor = XLColor.LightGray;
                    }

                    worksheet.Cell(currentRow, 7).Value = status;
                    worksheet.Cell(currentRow, 8).Value = item.VaccinationId;

                    // Formato condicional para estado
                    var statusCell = worksheet.Cell(currentRow, 7);
                    statusCell.Style.Fill.BackgroundColor = statusColor;
                    statusCell.Style.Font.FontColor = XLColor.White;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"HistorialVacunacion_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.Pet.PetName.Contains(searchString) ||
                    v.Vaccine.VaccineName.Contains(searchString) ||
                    (v.Veterinarian.User.FirstName + " " + v.Veterinarian.User.LastName).Contains(searchString) ||
                    v.BatchNumber!.Contains(searchString));
            }

            var vaccinations = await query
                .OrderByDescending(v => v.VaccinationDate)
                .Select(v => new VaccinationHistoryListViewModel
                {
                    VaccinationId = v.VaccinationId,
                    PetName = v.Pet.PetName,
                    VaccineName = v.Vaccine.VaccineName,
                    VeterinarianName = $"{v.Veterinarian.User.FirstName} {v.Veterinarian.User.LastName}",
                    VaccinationDate = v.VaccinationDate,
                    BatchNumber = v.BatchNumber!,
                    NextDueDate = v.NextDueDate
                })
                .ToListAsync();

            // Configuración del PDF con iText 7
            var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4.Rotate());

            // Fuentes
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título
            var title = new Paragraph("Reporte de Historial de Vacunación")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);

            // Fecha
            var date = new Paragraph($"Generado el: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(date);

            // Crear tabla
            var table = new Table(new float[] { 2, 2, 2, 2, 1, 2, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascota").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Vacuna").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Veterinario").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha Vacunación").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Lote").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Próxima Dosis").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in vaccinations)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.VaccineName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.VeterinarianName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.VaccinationDate.ToString("dd/MM/yyyy")).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.BatchNumber).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.NextDueDate?.ToString("dd/MM/yyyy") ?? "N/A").SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(
                    item.NextDueDate.HasValue ?
                        (item.NextDueDate.Value < DateTime.Today ? "Vencida" : "Pendiente") :
                        "No aplica")
                    .SetFont(normalFont));
                statusCell.SetBackgroundColor(
                    item.NextDueDate.HasValue && item.NextDueDate.Value < DateTime.Today ?
                        new DeviceRgb(220, 53, 69) :
                        new DeviceRgb(40, 167, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"HistorialVacunacion_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.Pet.PetName.Contains(searchString) ||
                    v.Vaccine.VaccineName.Contains(searchString) ||
                    (v.Veterinarian.User.FirstName + " " + v.Veterinarian.User.LastName).Contains(searchString) ||
                    v.BatchNumber!.Contains(searchString));
            }

            var vaccinations = await query
                .OrderByDescending(v => v.VaccinationDate)
                .Select(v => new VaccinationHistoryListViewModel
                {
                    VaccinationId = v.VaccinationId,
                    PetName = v.Pet.PetName,
                    VaccineName = v.Vaccine.VaccineName,
                    VeterinarianName = $"{v.Veterinarian.User.FirstName} {v.Veterinarian.User.LastName}",
                    VaccinationDate = v.VaccinationDate,
                    BatchNumber = v.BatchNumber!,
                    NextDueDate = v.NextDueDate
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(vaccinations);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var vaccination = await _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .FirstOrDefaultAsync(v => v.VaccinationId == id);

            if (vaccination == null) return NotFound();

            // Configuración del PDF
            var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4);

            // Fuentes
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Título
            var title = new Paragraph("CERTIFICADO DE VACUNACIÓN")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Subtítulo
            var subtitle = new Paragraph($"N° {vaccination.VaccinationId}")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(subtitle);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Mascota:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccination.Pet.PetName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Vacuna:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccination.Vaccine.VaccineName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Tipo de vacuna:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccination.Vaccine.VaccineType ?? "No especificado", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Fabricante:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccination.Vaccine.Manufacturer ?? "No especificado", normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Detalles de la vacunación
            var vaccinationTitle = new Paragraph("DETALLES DE LA APLICACIÓN")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(vaccinationTitle);

            var vaccinationTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            vaccinationTable.AddCell(CreateCell("Fecha de vacunación:", boldFont, TextAlignment.LEFT));
            vaccinationTable.AddCell(CreateCell(vaccination.VaccinationDate.ToString("dd/MM/yyyy"), normalFont, TextAlignment.LEFT));

            vaccinationTable.AddCell(CreateCell("Veterinario:", boldFont, TextAlignment.LEFT));
            vaccinationTable.AddCell(CreateCell(
                $"{vaccination.Veterinarian.User.FirstName} {vaccination.Veterinarian.User.LastName}",
                normalFont, TextAlignment.LEFT));

            vaccinationTable.AddCell(CreateCell("Número de lote:", boldFont, TextAlignment.LEFT));
            vaccinationTable.AddCell(CreateCell(vaccination.BatchNumber ?? "No especificado", normalFont, TextAlignment.LEFT));

            vaccinationTable.AddCell(CreateCell("Fecha de expiración:", boldFont, TextAlignment.LEFT));
            vaccinationTable.AddCell(CreateCell(
                vaccination.ExpirationDate?.ToString("dd/MM/yyyy") ?? "No especificado",
                normalFont, TextAlignment.LEFT));

            document.Add(vaccinationTable);

            // Próxima dosis y estado
            var nextDoseTitle = new Paragraph("PRÓXIMA DOSIS")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(nextDoseTitle);

            var nextDoseTable = new Table(new float[] { 1, 1 })
                .SetWidth(UnitValue.CreatePercentValue(50))
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .SetMarginBottom(20);

            nextDoseTable.AddCell(CreateCell("Fecha recomendada:", boldFont, TextAlignment.LEFT));
            nextDoseTable.AddCell(CreateCell(
                vaccination.NextDueDate?.ToString("dd/MM/yyyy") ?? "No aplica",
                normalFont, TextAlignment.LEFT));

            nextDoseTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(
                vaccination.NextDueDate.HasValue ?
                    (vaccination.NextDueDate.Value < DateTime.Today ? "Vencida" : "Pendiente") :
                    "No aplica",
                normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(
                vaccination.NextDueDate.HasValue && vaccination.NextDueDate.Value < DateTime.Today ?
                    new DeviceRgb(220, 53, 69) : new DeviceRgb(25, 135, 84));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            nextDoseTable.AddCell(statusCell);

            document.Add(nextDoseTable);

            // Reacciones
            if (!string.IsNullOrEmpty(vaccination.Reactions))
            {
                var reactionsTitle = new Paragraph("REACCIONES REGISTRADAS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(reactionsTitle);

                var reactionsContent = new Paragraph(vaccination.Reactions)
                    .SetFont(normalFont)
                    .SetMarginBottom(20)
                    .SetPaddingLeft(20)
                    .SetPaddingRight(20);
                document.Add(reactionsContent);
            }

            // Pie de página
            var footer = new Paragraph($"Certificado generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {vaccination.VaccinationId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"CertificadoVacunacion_{vaccination.Pet.PetName}_{vaccination.VaccinationDate:yyyyMMdd}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        public async Task<IActionResult> ExportDetailsToImage(int id, string format = "png")
        {
            var vaccination = await _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .FirstOrDefaultAsync(v => v.VaccinationId == id);

            if (vaccination == null) return NotFound();

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/VaccinationHistories/Details/{id}?exporting=true";

            // Configurar Puppeteer
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" }
            });

            await using var page = await browser.NewPageAsync();

            // Configurar la vista para exportación
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1200,
                Height = 900,
                DeviceScaleFactor = 2
            });

            await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);
            await page.WaitForSelectorAsync(".card");

            // Tomar captura de pantalla
            byte[] imageBytes;

            if (format.ToLower() == "jpg" || format.ToLower() == "jpeg")
            {
                imageBytes = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Type = ScreenshotType.Jpeg,
                    Quality = 90,
                    FullPage = true
                });
            }
            else
            {
                imageBytes = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Type = ScreenshotType.Png,
                    FullPage = true
                });
            }

            var contentType = format.ToLower() == "jpg" || format.ToLower() == "jpeg"
                ? "image/jpeg"
                : "image/png";

            var fileExtension = format.ToLower() == "jpg" || format.ToLower() == "jpeg"
                ? "jpg"
                : "png";

            return File(imageBytes, contentType,
                $"CertificadoVacunacion_{vaccination.Pet.PetName}_{vaccination.VaccinationDate:yyyyMMdd}.{fileExtension}");
        }

        // GET: VaccinationHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vaccination = await _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .FirstOrDefaultAsync(m => m.VaccinationId == id);

            if (vaccination == null) return NotFound();

            var viewModel = new VaccinationHistoryListViewModel
            {
                VaccinationId = vaccination.VaccinationId,
                PetName = vaccination.Pet.PetName,
                VaccineName = vaccination.Vaccine.VaccineName,
                VeterinarianName = $"{vaccination.Veterinarian.User.FirstName} {vaccination.Veterinarian.User.LastName}",
                VaccinationDate = vaccination.VaccinationDate,
                BatchNumber = vaccination.BatchNumber!,
                NextDueDate = vaccination.NextDueDate
            };

            ViewBag.ExpirationDate = vaccination.ExpirationDate?.ToString("dd/MM/yyyy");
            ViewBag.Reactions = vaccination.Reactions;
            ViewBag.VaccineType = vaccination.Vaccine.VaccineType;
            ViewBag.VaccineManufacturer = vaccination.Vaccine.Manufacturer;

            return View(viewModel);
        }

        // GET: VaccinationHistories/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new VaccinationHistoryFormViewModel
            {
                VaccinationDate = DateTime.Today
            });
        }

        // POST: VaccinationHistories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VaccinationHistoryFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vaccination = new VaccinationHistory
                    {
                        PetId = model.PetId,
                        VaccineId = model.VaccineId,
                        VeterinarianId = model.VeterinarianId,
                        VaccinationDate = model.VaccinationDate,
                        BatchNumber = model.BatchNumber,
                        ExpirationDate = model.ExpirationDate,
                        NextDueDate = model.NextDueDate,
                        Reactions = model.Reactions
                    };

                    _context.Add(vaccination);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vacunación registrada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al registrar vacunación");
                    ModelState.AddModelError("", "No se pudo registrar la vacunación. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: VaccinationHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccination = await _context.VaccinationHistories.FindAsync(id);
            if (vaccination == null)
            {
                return NotFound();
            }

            var model = new VaccinationHistoryFormViewModel
            {
                VaccinationId = vaccination.VaccinationId,
                PetId = vaccination.PetId,
                VaccineId = vaccination.VaccineId,
                VeterinarianId = vaccination.VeterinarianId,
                VaccinationDate = vaccination.VaccinationDate,
                BatchNumber = vaccination.BatchNumber!,
                ExpirationDate = vaccination.ExpirationDate,
                NextDueDate = vaccination.NextDueDate,
                Reactions = vaccination.Reactions!
            };

            await LoadViewData();
            ViewBag.PetName = (await _context.Pets.FindAsync(vaccination.PetId))?.PetName;
            ViewBag.VaccineName = (await _context.Vaccines.FindAsync(vaccination.VaccineId))?.VaccineName;
            ViewBag.VeterinarianName = (await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VeterinarianId == vaccination.VeterinarianId))?.User.FirstName;

            return View(model);
        }

        // POST: VaccinationHistories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VaccinationHistoryFormViewModel model)
        {
            if (id != model.VaccinationId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var vaccination = await _context.VaccinationHistories.FindAsync(id);
                    if (vaccination == null) return NotFound();

                    vaccination.VaccinationDate = model.VaccinationDate;
                    vaccination.BatchNumber = model.BatchNumber;
                    vaccination.ExpirationDate = model.ExpirationDate;
                    vaccination.NextDueDate = model.NextDueDate;
                    vaccination.Reactions = model.Reactions;

                    _context.Update(vaccination);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vacunación actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar vacunación");
                    ModelState.AddModelError("", "No se pudo actualizar la vacunación. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: VaccinationHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var vaccination = await _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .FirstOrDefaultAsync(m => m.VaccinationId == id);

            if (vaccination == null) return NotFound();

            return View(vaccination);
        }

        // POST: VaccinationHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vaccination = await _context.VaccinationHistories.FindAsync(id);
            if (vaccination != null)
            {
                _context.VaccinationHistories.Remove(vaccination);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vacunación eliminada exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewData()
        {
            // Cargar mascotas activas
            ViewBag.Pets = await _context.Pets
                .Where(p => p.IsActive)
                .OrderBy(p => p.PetName)
                .Select(p => new
                {
                    PetId = p.PetId,
                    DisplayText = $"{p.PetName} ({p.Species.SpeciesName})"
                })
                .ToListAsync();

            // Cargar vacunas activas
            ViewBag.Vaccines = await _context.Vaccines
                .Where(v => v.IsActive)
                .OrderBy(v => v.VaccineName)
                .Select(v => new
                {
                    VaccineId = v.VaccineId,
                    DisplayText = v.VaccineName
                })
                .ToListAsync();

            // Cargar veterinarios activos
            ViewBag.Veterinarians = await _context.Veterinarians
                .Include(v => v.User)
                .Where(v => v.User.Role.IsActive)
                .OrderBy(v => v.User.LastName)
                .Select(v => new
                {
                    VeterinarianId = v.VeterinarianId,
                    DisplayText = $"{v.User.FirstName} {v.User.LastName}"
                })
                .ToListAsync();
        }
    }
}
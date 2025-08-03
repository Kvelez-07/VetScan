// Controllers/MedicationsController.cs
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
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class MedicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicationsController> _logger;

        public MedicationsController(ApplicationDbContext context, ILogger<MedicationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Medications
                .Where(m => m.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m =>
                    m.MedicationName.Contains(searchString) ||
                    (m.Manufacturer != null && m.Manufacturer.Contains(searchString)));
            }

            var medications = await query
                .OrderBy(m => m.MedicationName)
                .Select(m => new MedicationListViewModel
                {
                    MedicationId = m.MedicationId,
                    MedicationName = m.MedicationName,
                    GenericName = m.GenericName,
                    Manufacturer = m.Manufacturer,
                    Concentration = m.Concentration,
                    Category = m.Category
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Medicamentos");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Nombre Comercial";
                worksheet.Cell(currentRow, 2).Value = "Nombre Genérico";
                worksheet.Cell(currentRow, 3).Value = "Fabricante";
                worksheet.Cell(currentRow, 4).Value = "Concentración";
                worksheet.Cell(currentRow, 5).Value = "Categoría";
                worksheet.Cell(currentRow, 6).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 6);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in medications)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.MedicationName;
                    worksheet.Cell(currentRow, 2).Value = item.GenericName ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = item.Manufacturer ?? "N/A";
                    worksheet.Cell(currentRow, 4).Value = item.Concentration ?? "N/A";
                    worksheet.Cell(currentRow, 5).Value = item.Category ?? "N/A";
                    worksheet.Cell(currentRow, 6).Value = item.MedicationId;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Medicamentos_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Medications
                .Where(m => m.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m =>
                    m.MedicationName.Contains(searchString) ||
                    (m.Manufacturer != null && m.Manufacturer.Contains(searchString)));
            }

            var medications = await query
                .OrderBy(m => m.MedicationName)
                .Select(m => new MedicationListViewModel
                {
                    MedicationId = m.MedicationId,
                    MedicationName = m.MedicationName,
                    GenericName = m.GenericName,
                    Manufacturer = m.Manufacturer,
                    Concentration = m.Concentration,
                    Category = m.Category
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
            var title = new Paragraph("Reporte de Medicamentos")
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
            var table = new Table(new float[] { 2, 2, 2, 2, 2 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre Comercial").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre Genérico").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fabricante").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Concentración").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Categoría").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var med in medications)
            {
                table.AddCell(new Cell().Add(new Paragraph(med.MedicationName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(med.GenericName ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(med.Manufacturer ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(med.Concentration ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(med.Category ?? "N/A").SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Medicamentos_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: Medications
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.Medications
                .Where(m => m.IsActive);

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m =>
                    m.MedicationName.Contains(searchString) ||
                    (m.Manufacturer != null && m.Manufacturer.Contains(searchString)));
            }

            var medications = await query
                .OrderBy(m => m.MedicationName)
                .Select(m => new MedicationListViewModel
                {
                    MedicationId = m.MedicationId,
                    MedicationName = m.MedicationName,
                    GenericName = m.GenericName,
                    Manufacturer = m.Manufacturer,
                    Concentration = m.Concentration,
                    Category = m.Category
                })
                .ToListAsync();

            // Pasar el término de búsqueda a la vista
            ViewData["CurrentFilter"] = searchString;

            return View(medications);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.MedicationId == id && m.IsActive);

            if (medication == null) return NotFound();

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
            var title = new Paragraph($"Detalles de Medicamento: {medication.MedicationName}")
                .SetFont(headerFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Nombre Comercial:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medication.MedicationName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Nombre Genérico:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medication.GenericName ?? "N/A", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Fabricante:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medication.Manufacturer ?? "N/A", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Concentración:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medication.Concentration ?? "N/A", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Categoría:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medication.Category ?? "N/A", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(medication.IsActive ? "Activo" : "Inactivo", normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(medication.IsActive ?
                new DeviceRgb(40, 167, 69) : new DeviceRgb(108, 117, 125));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            infoTable.AddCell(CreateCell("Fecha de Creación:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medication.CreatedDate.ToString("dd/MM/yyyy HH:mm"), normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Pie de página
            var footer = new Paragraph($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {medication.MedicationId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf", $"Medicamento_{medication.MedicationName}_{DateTime.Now:yyyyMMdd}.pdf");
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
            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.MedicationId == id && m.IsActive);

            if (medication == null) return NotFound();

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/Medications/Details/{id}?exporting=true";

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
                Height = 1000,
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
                $"Medicamento_{medication.MedicationName}_{DateTime.Now:yyyyMMdd}.{fileExtension}");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.MedicationId == id && m.IsActive);

            if (medication == null) return NotFound();

            var viewModel = new MedicationListViewModel
            {
                MedicationId = medication.MedicationId,
                MedicationName = medication.MedicationName,
                GenericName = medication.GenericName,
                Manufacturer = medication.Manufacturer,
                Concentration = medication.Concentration,
                Category = medication.Category
            };

            // Agregar datos adicionales que no están en el ListViewModel
            ViewBag.CreatedDate = medication.CreatedDate.ToString("dd/MM/yyyy HH:mm");
            ViewBag.IsActive = medication.IsActive ? "Activo" : "Inactivo";

            return View(viewModel);
        }

        // GET: Medications/Create
        public IActionResult Create()
        {
            return View(new MedicationFormViewModel());
        }

        // POST: Medications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicationFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var medication = new Medication
                    {
                        MedicationName = model.MedicationName,
                        GenericName = model.GenericName,
                        Manufacturer = model.Manufacturer,
                        Concentration = model.Concentration,
                        Category = model.Category,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(medication);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Medicamento creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear medicamento");
                    ModelState.AddModelError("", "No se pudo guardar el medicamento. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: Medications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();

            var model = new MedicationFormViewModel
            {
                MedicationId = medication.MedicationId,
                MedicationName = medication.MedicationName,
                GenericName = medication.GenericName,
                Manufacturer = medication.Manufacturer,
                Concentration = medication.Concentration,
                Category = medication.Category
            };

            return View(model);
        }

        // POST: Medications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicationFormViewModel model)
        {
            if (id != model.MedicationId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var medication = await _context.Medications.FindAsync(id);
                    if (medication == null) return NotFound();

                    medication.MedicationName = model.MedicationName;
                    medication.GenericName = model.GenericName;
                    medication.Manufacturer = model.Manufacturer;
                    medication.Concentration = model.Concentration;
                    medication.Category = model.Category;

                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Medicamento actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar medicamento");
                    ModelState.AddModelError("", "No se pudo actualizar el medicamento. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // POST: Medications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                medication.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medicamento eliminado exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
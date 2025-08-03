// Controllers/MedicalRecordsController.cs
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
    public class MedicalRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(ApplicationDbContext context, ILogger<MedicalRecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            IQueryable<MedicalRecord> baseQuery = _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User);

            if (!string.IsNullOrEmpty(searchString))
            {
                baseQuery = baseQuery.Where(mr =>
                    mr.RecordNumber.Contains(searchString) ||
                    mr.Pet.PetName.Contains(searchString));
            }

            var medicalRecords = await baseQuery.OrderByDescending(mr => mr.CreationDate)
                .Select(mr => new MedicalRecordListViewModel
                {
                    MedicalRecordId = mr.MedicalRecordId,
                    RecordNumber = mr.RecordNumber,
                    PetName = mr.Pet.PetName,
                    PetId = mr.PetId,
                    OwnerName = $"{mr.Pet.PetOwner.User.FirstName} {mr.Pet.PetOwner.User.LastName}",
                    CreationDate = mr.CreationDate,
                    Status = mr.Status
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("RegistrosMedicos");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Número";
                worksheet.Cell(currentRow, 2).Value = "Mascota";
                worksheet.Cell(currentRow, 3).Value = "Dueño";
                worksheet.Cell(currentRow, 4).Value = "Fecha Creación";
                worksheet.Cell(currentRow, 5).Value = "Estado";
                worksheet.Cell(currentRow, 6).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 6);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in medicalRecords)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.RecordNumber;
                    worksheet.Cell(currentRow, 2).Value = item.PetName;
                    worksheet.Cell(currentRow, 3).Value = item.OwnerName;
                    worksheet.Cell(currentRow, 4).Value = item.FormattedCreationDate;
                    worksheet.Cell(currentRow, 5).Value = item.Status;
                    worksheet.Cell(currentRow, 6).Value = item.MedicalRecordId;

                    // Color para estado
                    var statusCell = worksheet.Cell(currentRow, 5);
                    statusCell.Style.Fill.BackgroundColor =
                        item.Status == "Active" ? XLColor.Green : XLColor.Red;
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
                        $"RegistrosMedicos_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            IQueryable<MedicalRecord> baseQuery = _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User);

            if (!string.IsNullOrEmpty(searchString))
            {
                baseQuery = baseQuery.Where(mr =>
                    mr.RecordNumber.Contains(searchString) ||
                    mr.Pet.PetName.Contains(searchString));
            }

            var medicalRecords = await baseQuery.OrderByDescending(mr => mr.CreationDate)
                .Select(mr => new MedicalRecordListViewModel
                {
                    MedicalRecordId = mr.MedicalRecordId,
                    RecordNumber = mr.RecordNumber,
                    PetName = mr.Pet.PetName,
                    PetId = mr.PetId,
                    OwnerName = $"{mr.Pet.PetOwner.User.FirstName} {mr.Pet.PetOwner.User.LastName}",
                    CreationDate = mr.CreationDate,
                    Status = mr.Status
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
            var title = new Paragraph("Reporte de Registros Médicos")
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
            table.AddHeaderCell(new Cell().Add(new Paragraph("Número").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascota").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Dueño").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha Creación").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var record in medicalRecords)
            {
                table.AddCell(new Cell().Add(new Paragraph(record.RecordNumber).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(record.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(record.OwnerName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(record.FormattedCreationDate).SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(record.Status).SetFont(normalFont));
                statusCell.SetBackgroundColor(
                    record.Status == "Active" ? new DeviceRgb(40, 167, 69) :
                    new DeviceRgb(220, 53, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"RegistrosMedicos_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base sin ordenar inicialmente
            IQueryable<MedicalRecord> baseQuery = _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User);

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                baseQuery = baseQuery.Where(mr =>
                    mr.RecordNumber.Contains(searchString) ||
                    mr.Pet.PetName.Contains(searchString));
            }

            // Ordenar después de aplicar filtros
            var orderedQuery = baseQuery.OrderByDescending(mr => mr.CreationDate);

            var medicalRecords = await orderedQuery
                .Select(mr => new MedicalRecordListViewModel
                {
                    MedicalRecordId = mr.MedicalRecordId,
                    RecordNumber = mr.RecordNumber,
                    PetName = mr.Pet.PetName,
                    PetId = mr.PetId,
                    OwnerName = $"{mr.Pet.PetOwner.User.FirstName} {mr.Pet.PetOwner.User.LastName}",
                    CreationDate = mr.CreationDate,
                    Status = mr.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(medicalRecords);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null) return NotFound();

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
            var title = new Paragraph($"Registro Médico: {medicalRecord.RecordNumber}")
                .SetFont(headerFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Número de Registro:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medicalRecord.RecordNumber, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Fecha de Creación:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(medicalRecord.CreationDate.ToString("dd/MM/yyyy HH:mm"), normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(medicalRecord.Status, normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(medicalRecord.Status == "Active" ?
                new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            infoTable.AddCell(CreateCell("Mascota:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell($"{medicalRecord.Pet.PetName} ({medicalRecord.Pet.PetCode})", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Dueño:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(
                $"{medicalRecord.Pet.PetOwner.User.FirstName} {medicalRecord.Pet.PetOwner.User.LastName}",
                normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Notas generales
            var notesTitle = new Paragraph("Notas Generales")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(notesTitle);

            var notesContent = new Paragraph(string.IsNullOrEmpty(medicalRecord.GeneralNotes) ?
                "No hay notas registradas" : medicalRecord.GeneralNotes)
                .SetFont(normalFont)
                .SetMarginBottom(20);
            document.Add(notesContent);

            // Pie de página
            var footer = new Paragraph($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {medicalRecord.MedicalRecordId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf", $"RegistroMedico_{medicalRecord.RecordNumber}_{DateTime.Now:yyyyMMdd}.pdf");
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
            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null) return NotFound();

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/MedicalRecords/Details/{id}?exporting=true";

            // Configurar Puppeteer
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" } // Necesario para algunos entornos
            });

            await using var page = await browser.NewPageAsync();

            // Configurar la vista para exportación (opcional)
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1200,
                Height = 1600,
                DeviceScaleFactor = 2 // Para mejor calidad
            });

            await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

            // Opcional: Esperar a que ciertos elementos estén cargados
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
                $"RegistroMedico_{medicalRecord.RecordNumber}_{DateTime.Now:yyyyMMdd}.{fileExtension}");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null) return NotFound();

            var viewModel = new MedicalRecordListViewModel
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                RecordNumber = medicalRecord.RecordNumber,
                PetName = medicalRecord.Pet.PetName,
                PetId = medicalRecord.PetId,
                OwnerName = $"{medicalRecord.Pet.PetOwner.User.FirstName} {medicalRecord.Pet.PetOwner.User.LastName}",
                CreationDate = medicalRecord.CreationDate,
                Status = medicalRecord.Status
            };

            // Agregar datos adicionales que no están en el ListViewModel
            ViewBag.GeneralNotes = medicalRecord.GeneralNotes;
            ViewBag.PetCode = medicalRecord.Pet.PetCode;

            return View(viewModel);
        }

        // GET: MedicalRecords/Create
        public async Task<IActionResult> Create()
        {
            await LoadPetsViewData();
            return View(new MedicalRecordFormViewModel
            {
                RecordNumber = GenerateRecordNumber(),
                Status = "Active"
            });
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecordFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = new MedicalRecord
                    {
                        PetId = model.PetId,
                        RecordNumber = model.RecordNumber,
                        GeneralNotes = model.GeneralNotes,
                        Status = model.Status,
                        CreationDate = DateTime.UtcNow
                    };

                    _context.Add(medicalRecord);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Registro médico creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear registro médico");
                    ModelState.AddModelError("", "No se pudo crear el registro médico. Intente nuevamente.");
                }
            }

            await LoadPetsViewData();
            return View(model);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null) return NotFound();

            await LoadPetsViewData();

            var model = new MedicalRecordFormViewModel
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                PetId = medicalRecord.PetId,
                RecordNumber = medicalRecord.RecordNumber,
                GeneralNotes = medicalRecord.GeneralNotes,
                Status = medicalRecord.Status
            };

            ViewBag.PetName = medicalRecord.Pet.PetName;
            return View(model);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalRecordFormViewModel model)
        {
            if (id != model.MedicalRecordId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = await _context.MedicalRecords.FindAsync(id);
                    if (medicalRecord == null) return NotFound();

                    medicalRecord.RecordNumber = model.RecordNumber;
                    medicalRecord.GeneralNotes = model.GeneralNotes;
                    medicalRecord.Status = model.Status;

                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Registro médico actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar registro médico");
                    ModelState.AddModelError("", "No se pudo actualizar el registro médico. Intente nuevamente.");
                }
            }

            await LoadPetsViewData();
            return View(model);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null) return NotFound();

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
            {
                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registro médico eliminado exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPetsViewData()
        {
            ViewBag.Pets = await _context.Pets
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    PetId = p.PetId,
                    DisplayText = $"{p.PetName} (Dueño: {p.PetOwner.User.FirstName} {p.PetOwner.User.LastName})"
                })
                .ToListAsync();
        }

        private string GenerateRecordNumber() => $"MR-{DateTime.Now:yyyyMMddHHmmss}";
    }
}
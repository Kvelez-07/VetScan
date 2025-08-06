// Controllers/PrescriptionsController.cs
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
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(ApplicationDbContext context, ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            var query = _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .OrderByDescending(p => p.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.Consultation.MedicalRecord.Pet.PetName.Contains(searchString) ||
                    p.Medication.MedicationName.Contains(searchString));
            }

            var prescriptions = await query
                .Select(p => new PrescriptionListViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    ConsultationId = p.ConsultationId,
                    ConsultationInfo = $"Consulta del {p.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = p.Consultation.MedicalRecord.Pet.PetName,
                    MedicationName = p.Medication.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    CreatedDate = p.CreatedDate,
                    Status = p.Status
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Prescripciones");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Fecha";
                worksheet.Cell(currentRow, 2).Value = "Mascota";
                worksheet.Cell(currentRow, 3).Value = "Consulta";
                worksheet.Cell(currentRow, 4).Value = "Medicamento";
                worksheet.Cell(currentRow, 5).Value = "Dosis";
                worksheet.Cell(currentRow, 6).Value = "Frecuencia";
                worksheet.Cell(currentRow, 7).Value = "Estado";
                worksheet.Cell(currentRow, 8).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 8);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in prescriptions)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.FormattedDate;
                    worksheet.Cell(currentRow, 2).Value = item.PetName;
                    worksheet.Cell(currentRow, 3).Value = item.ConsultationInfo;
                    worksheet.Cell(currentRow, 4).Value = item.MedicationName;
                    worksheet.Cell(currentRow, 5).Value = item.Dosage;
                    worksheet.Cell(currentRow, 6).Value = item.Frequency;
                    worksheet.Cell(currentRow, 7).Value = item.Status;
                    worksheet.Cell(currentRow, 8).Value = item.PrescriptionId;

                    // Color para estado
                    var statusCell = worksheet.Cell(currentRow, 7);
                    statusCell.Style.Fill.BackgroundColor =
                        item.Status == "Active" ? XLColor.Green :
                        item.Status == "Completed" ? XLColor.Blue :
                        item.Status == "Cancelled" ? XLColor.Red :
                        XLColor.Gray;
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
                        $"Prescripciones_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .OrderByDescending(p => p.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.Consultation.MedicalRecord.Pet.PetName.Contains(searchString) ||
                    p.Medication.MedicationName.Contains(searchString));
            }

            var prescriptions = await query
                .Select(p => new PrescriptionListViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    ConsultationId = p.ConsultationId,
                    ConsultationInfo = $"Consulta del {p.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = p.Consultation.MedicalRecord.Pet.PetName,
                    MedicationName = p.Medication.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    CreatedDate = p.CreatedDate,
                    Status = p.Status
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
            var title = new Paragraph("Reporte de Prescripciones")
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
            var table = new Table(new float[] { 2, 2, 2, 2, 2, 2, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascota").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Consulta").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Medicamento").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Dosis").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Frecuencia").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in prescriptions)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedDate).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.ConsultationInfo).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.MedicationName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Dosage).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Frequency).SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(item.Status).SetFont(normalFont));
                statusCell.SetBackgroundColor(
                    item.Status == "Active" ? new DeviceRgb(25, 135, 84) :
                    new DeviceRgb(108, 117, 125));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Prescripciones_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: Prescriptions
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .OrderByDescending(p => p.CreatedDate)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.Consultation.MedicalRecord.Pet.PetName.Contains(searchString) ||
                    p.Medication.MedicationName.Contains(searchString));
            }

            var prescriptions = await query
                .Select(p => new PrescriptionListViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    ConsultationId = p.ConsultationId,
                    ConsultationInfo = $"Consulta del {p.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = p.Consultation.MedicalRecord.Pet.PetName,
                    MedicationName = p.Medication.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    CreatedDate = p.CreatedDate,
                    Status = p.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(prescriptions);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null) return NotFound();

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
            var title = new Paragraph($"PRESCRIPCIÓN MÉDICA")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Sub título
            var subtitle = new Paragraph($"N° {prescription.PrescriptionId}")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(subtitle);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Fecha de prescripción:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(prescription.CreatedDate.ToString("dd/MM/yyyy HH:mm"), normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Mascota:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(prescription.Consultation.MedicalRecord.Pet.PetName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Veterinario:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(
                $"{prescription.Consultation.AttendingVeterinarian.User.FirstName} {prescription.Consultation.AttendingVeterinarian.User.LastName}",
                normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Consulta asociada:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(
                $"Del {prescription.Consultation.ConsultationDate:dd/MM/yyyy}",
                normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Detalles de la prescripción
            var prescriptionTitle = new Paragraph("DETALLES DE LA MEDICACIÓN")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(prescriptionTitle);

            var prescriptionTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            prescriptionTable.AddCell(CreateCell("Medicamento:", boldFont, TextAlignment.LEFT));
            prescriptionTable.AddCell(CreateCell(prescription.Medication.MedicationName, normalFont, TextAlignment.LEFT));

            prescriptionTable.AddCell(CreateCell("Dosis:", boldFont, TextAlignment.LEFT));
            prescriptionTable.AddCell(CreateCell(prescription.Dosage, normalFont, TextAlignment.LEFT));

            prescriptionTable.AddCell(CreateCell("Frecuencia:", boldFont, TextAlignment.LEFT));
            prescriptionTable.AddCell(CreateCell(prescription.Frequency, normalFont, TextAlignment.LEFT));

            prescriptionTable.AddCell(CreateCell("Duración:", boldFont, TextAlignment.LEFT));
            prescriptionTable.AddCell(CreateCell(prescription.Duration ?? "N/A", normalFont, TextAlignment.LEFT));

            prescriptionTable.AddCell(CreateCell("Cantidad:", boldFont, TextAlignment.LEFT));
            prescriptionTable.AddCell(CreateCell(prescription.Quantity?.ToString() ?? "N/A", normalFont, TextAlignment.LEFT));

            prescriptionTable.AddCell(CreateCell("Reabastecimientos:", boldFont, TextAlignment.LEFT));
            prescriptionTable.AddCell(CreateCell(prescription.Refills.ToString(), normalFont, TextAlignment.LEFT));

            document.Add(prescriptionTable);

            // Periodo de validez
            if (prescription.StartDate.HasValue || prescription.EndDate.HasValue)
            {
                var periodTitle = new Paragraph("PERIODO DE VALIDEZ")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(periodTitle);

                var periodTable = new Table(new float[] { 1, 1 })
                    .SetWidth(UnitValue.CreatePercentValue(50))
                    .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                    .SetMarginBottom(20);

                periodTable.AddCell(CreateCell("Fecha de inicio:", boldFont, TextAlignment.LEFT));
                periodTable.AddCell(CreateCell(
                    prescription.StartDate?.ToString("dd/MM/yyyy") ?? "N/A",
                    normalFont, TextAlignment.LEFT));

                periodTable.AddCell(CreateCell("Fecha de fin:", boldFont, TextAlignment.LEFT));
                periodTable.AddCell(CreateCell(
                    prescription.EndDate?.ToString("dd/MM/yyyy") ?? "N/A",
                    normalFont, TextAlignment.LEFT));

                document.Add(periodTable);
            }

            // Instrucciones
            if (!string.IsNullOrEmpty(prescription.Instructions))
            {
                var instructionsTitle = new Paragraph("INSTRUCCIONES ESPECIALES")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(instructionsTitle);

                var instructionsContent = new Paragraph(prescription.Instructions)
                    .SetFont(normalFont)
                    .SetMarginBottom(20)
                    .SetPaddingLeft(20)
                    .SetPaddingRight(20);
                document.Add(instructionsContent);
            }

            // Estado
            var statusCell = CreateCell(prescription.Status, normalFont, TextAlignment.CENTER);
            statusCell.SetBackgroundColor(
                prescription.Status == "Active" ? new DeviceRgb(25, 135, 84) :
                prescription.Status == "Completed" ? new DeviceRgb(13, 110, 253) :
                prescription.Status == "Cancelled" ? new DeviceRgb(220, 53, 69) :
                new DeviceRgb(108, 117, 125));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            statusCell.SetMarginBottom(20);
            document.Add(statusCell);

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {prescription.PrescriptionId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Prescripcion_{prescription.Consultation.MedicalRecord.Pet.PetName}_{prescription.CreatedDate:yyyyMMdd}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // Método para exportar detalles a imagen
        public async Task<IActionResult> ExportDetailsToImage(int id, string format = "png")
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/Prescriptions/Details/{id}?exporting=true";

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
                Height = 800,
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
                $"Prescripcion_{prescription.Consultation.MedicalRecord.Pet.PetName}_{DateTime.Now:yyyyMMdd}.{fileExtension}");
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null) return NotFound();

            var viewModel = new PrescriptionListViewModel
            {
                PrescriptionId = prescription.PrescriptionId,
                ConsultationId = prescription.ConsultationId,
                ConsultationInfo = $"Consulta del {prescription.Consultation.ConsultationDate:dd/MM/yyyy}",
                PetName = prescription.Consultation.MedicalRecord.Pet.PetName,
                MedicationName = prescription.Medication.MedicationName,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                CreatedDate = prescription.CreatedDate,
                Status = prescription.Status
            };

            ViewBag.Duration = prescription.Duration;
            ViewBag.Instructions = prescription.Instructions;
            ViewBag.Quantity = prescription.Quantity;
            ViewBag.Refills = prescription.Refills;
            ViewBag.StartDate = prescription.StartDate?.ToString("dd/MM/yyyy");
            ViewBag.EndDate = prescription.EndDate?.ToString("dd/MM/yyyy");

            return View(viewModel);
        }

        // GET: Prescriptions/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new PrescriptionFormViewModel
            {
                StartDate = DateTime.Today,
                Status = "Active"
            });
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = new Prescription
                    {
                        ConsultationId = model.ConsultationId,
                        MedicationId = model.MedicationId,
                        Dosage = model.Dosage,
                        Frequency = model.Frequency,
                        Duration = model.Duration,
                        Instructions = model.Instructions,
                        Quantity = model.Quantity,
                        Refills = model.Refills,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Status = model.Status,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(prescription);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Prescripción creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear prescripción");
                    ModelState.AddModelError("", "No se pudo crear la prescripción. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Prescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null) return NotFound();

            var model = new PrescriptionFormViewModel
            {
                PrescriptionId = prescription.PrescriptionId,
                ConsultationId = prescription.ConsultationId,
                MedicationId = prescription.MedicationId,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                Duration = prescription.Duration,
                Instructions = prescription.Instructions,
                Quantity = prescription.Quantity,
                Refills = prescription.Refills,
                StartDate = prescription.StartDate,
                EndDate = prescription.EndDate,
                Status = prescription.Status
            };

            ViewBag.ConsultationInfo = $"Consulta del {prescription.Consultation.ConsultationDate:dd/MM/yyyy}";
            ViewBag.MedicationName = prescription.Medication.MedicationName;
            await LoadViewData();
            return View(model);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PrescriptionFormViewModel model)
        {
            if (id != model.PrescriptionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = await _context.Prescriptions.FindAsync(id);
                    if (prescription == null) return NotFound();

                    prescription.Dosage = model.Dosage;
                    prescription.Frequency = model.Frequency;
                    prescription.Duration = model.Duration;
                    prescription.Instructions = model.Instructions;
                    prescription.Quantity = model.Quantity;
                    prescription.Refills = model.Refills;
                    prescription.StartDate = model.StartDate;
                    prescription.EndDate = model.EndDate;
                    prescription.Status = model.Status;

                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Prescripción actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar prescripción");
                    ModelState.AddModelError("", "No se pudo actualizar la prescripción. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Prescriptions/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "ID de prescripción no proporcionado";
                return RedirectToAction(nameof(Index));
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescripción no encontrada";
                return RedirectToAction(nameof(Index));
            }

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                TempData["ErrorMessage"] = "Prescripción no encontrada";
                return RedirectToAction(nameof(Index));
            }

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Prescripción eliminada exitosamente";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewData()
        {
            // Cargar consultas médicas
            ViewBag.Consultations = await _context.MedicalConsultations
                .Include(c => c.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .OrderByDescending(c => c.ConsultationDate)
                .Select(c => new
                {
                    ConsultationId = c.ConsultationId,
                    DisplayText = $"{c.MedicalRecord.Pet.PetName} - {c.ConsultationDate:dd/MM/yyyy}"
                })
                .ToListAsync();

            // Cargar medicamentos activos
            ViewBag.Medications = await _context.Medications
                .Where(m => m.IsActive)
                .OrderBy(m => m.MedicationName)
                .Select(m => new
                {
                    MedicationId = m.MedicationId,
                    DisplayText = m.MedicationName
                })
                .ToListAsync();

            // Estados posibles
            ViewBag.StatusOptions = new List<string>
            {
                "Active",
                "Completed",
                "Cancelled",
                "Expired"
            };
        }
    }
}
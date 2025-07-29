// Controllers/VitalSignsController.cs
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
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class VitalSignsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VitalSignsController> _logger;

        public VitalSignsController(ApplicationDbContext context, ILogger<VitalSignsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .OrderByDescending(vs => vs.RecordedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(vs =>
                    vs.Consultation.MedicalRecord.Pet.PetName.Contains(searchString));
            }

            var vitalSigns = await query
                .Select(vs => new VitalSignListViewModel
                {
                    VitalSignId = vs.VitalSignId,
                    ConsultationId = vs.ConsultationId,
                    ConsultationInfo = $"Consulta del {vs.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = vs.Consultation.MedicalRecord.Pet.PetName,
                    RecordedDate = vs.RecordedDate,
                    Temperature = vs.Temperature,
                    HeartRate = vs.HeartRate,
                    RespiratoryRate = vs.RespiratoryRate,
                    Weight = vs.Weight,
                    BloodPressure = vs.BloodPressureSystolic.HasValue && vs.BloodPressureDiastolic.HasValue ?
                        $"{vs.BloodPressureSystolic}/{vs.BloodPressureDiastolic}" : "N/A"
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SignosVitales");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Fecha";
                worksheet.Cell(currentRow, 2).Value = "Mascota";
                worksheet.Cell(currentRow, 3).Value = "Consulta";
                worksheet.Cell(currentRow, 4).Value = "Temperatura (°C)";
                worksheet.Cell(currentRow, 5).Value = "Frec. Cardíaca (lpm)";
                worksheet.Cell(currentRow, 6).Value = "Frec. Respiratoria (rpm)";
                worksheet.Cell(currentRow, 7).Value = "Peso (kg)";
                worksheet.Cell(currentRow, 8).Value = "Presión Arterial";
                worksheet.Cell(currentRow, 9).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 9);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in vitalSigns)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.RecordedDate.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(currentRow, 2).Value = item.PetName;
                    worksheet.Cell(currentRow, 3).Value = item.ConsultationInfo;
                    worksheet.Cell(currentRow, 4).Value = item.Temperature;
                    worksheet.Cell(currentRow, 5).Value = item.HeartRate;
                    worksheet.Cell(currentRow, 6).Value = item.RespiratoryRate;
                    worksheet.Cell(currentRow, 7).Value = item.Weight;
                    worksheet.Cell(currentRow, 8).Value = item.BloodPressure;
                    worksheet.Cell(currentRow, 9).Value = item.VitalSignId;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"SignosVitales_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .OrderByDescending(vs => vs.RecordedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(vs =>
                    vs.Consultation.MedicalRecord.Pet.PetName.Contains(searchString));
            }

            var vitalSigns = await query
                .Select(vs => new VitalSignListViewModel
                {
                    VitalSignId = vs.VitalSignId,
                    ConsultationId = vs.ConsultationId,
                    ConsultationInfo = $"Consulta del {vs.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = vs.Consultation.MedicalRecord.Pet.PetName,
                    RecordedDate = vs.RecordedDate,
                    Temperature = vs.Temperature,
                    HeartRate = vs.HeartRate,
                    RespiratoryRate = vs.RespiratoryRate,
                    Weight = vs.Weight,
                    BloodPressure = vs.BloodPressureSystolic.HasValue && vs.BloodPressureDiastolic.HasValue ?
                        $"{vs.BloodPressureSystolic}/{vs.BloodPressureDiastolic}" : "N/A"
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
            var title = new Paragraph("Reporte de Signos Vitales")
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
            var table = new Table(new float[] { 2, 2, 2, 1, 1, 1, 1, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascota").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Consulta").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Temp (°C)").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Cardíaca (lpm)").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Resp. (rpm)").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Peso (kg)").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Presión").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in vitalSigns)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedDate).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.ConsultationInfo).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedTemperature).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedHeartRate).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedRespiratoryRate).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedWeight).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.BloodPressure).SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"SignosVitales_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .OrderByDescending(vs => vs.RecordedDate)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(vs =>
                    vs.Consultation.MedicalRecord.Pet.PetName.Contains(searchString));
            }

            var vitalSigns = await query
                .Select(vs => new VitalSignListViewModel
                {
                    VitalSignId = vs.VitalSignId,
                    ConsultationId = vs.ConsultationId,
                    ConsultationInfo = $"Consulta del {vs.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = vs.Consultation.MedicalRecord.Pet.PetName,
                    RecordedDate = vs.RecordedDate,
                    Temperature = vs.Temperature,
                    HeartRate = vs.HeartRate,
                    RespiratoryRate = vs.RespiratoryRate,
                    Weight = vs.Weight,
                    BloodPressure = vs.BloodPressureSystolic.HasValue && vs.BloodPressureDiastolic.HasValue ?
                        $"{vs.BloodPressureSystolic}/{vs.BloodPressureDiastolic}" : "N/A"
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(vitalSigns);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(vs => vs.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

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
            var title = new Paragraph($"Signos Vitales - {vitalSign.RecordedDate:dd/MM/yyyy HH:mm}")
                .SetFont(headerFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Fecha:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vitalSign.RecordedDate.ToString("dd/MM/yyyy HH:mm"), normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Mascota:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vitalSign.Consultation.MedicalRecord.Pet.PetName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Consulta:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(
                $"Del {vitalSign.Consultation.ConsultationDate:dd/MM/yyyy} con {vitalSign.Consultation.AttendingVeterinarian.User.FirstName} {vitalSign.Consultation.AttendingVeterinarian.User.LastName}",
                normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Signos vitales
            var signsTitle = new Paragraph("Mediciones")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(signsTitle);

            var signsTable = new Table(new float[] { 4, 6 })
                .SetWidth(UnitValue.CreatePercentValue(80))
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .SetMarginBottom(20);

            signsTable.AddCell(CreateCell("Temperatura:", boldFont, TextAlignment.LEFT));
            signsTable.AddCell(CreateCell($"{vitalSign.Temperature} °C", normalFont, TextAlignment.LEFT));

            signsTable.AddCell(CreateCell("Frecuencia Cardíaca:", boldFont, TextAlignment.LEFT));
            signsTable.AddCell(CreateCell($"{vitalSign.HeartRate} lpm", normalFont, TextAlignment.LEFT));

            signsTable.AddCell(CreateCell("Frecuencia Respiratoria:", boldFont, TextAlignment.LEFT));
            signsTable.AddCell(CreateCell($"{vitalSign.RespiratoryRate} rpm", normalFont, TextAlignment.LEFT));

            signsTable.AddCell(CreateCell("Peso:", boldFont, TextAlignment.LEFT));
            signsTable.AddCell(CreateCell($"{vitalSign.Weight} kg", normalFont, TextAlignment.LEFT));

            if (vitalSign.BloodPressureSystolic.HasValue && vitalSign.BloodPressureDiastolic.HasValue)
            {
                signsTable.AddCell(CreateCell("Presión Arterial:", boldFont, TextAlignment.LEFT));
                signsTable.AddCell(CreateCell(
                    $"{vitalSign.BloodPressureSystolic}/{vitalSign.BloodPressureDiastolic} mmHg",
                    normalFont, TextAlignment.LEFT));
            }

            document.Add(signsTable);

            // Notas adicionales
            if (!string.IsNullOrEmpty(vitalSign.Notes))
            {
                var notesTitle = new Paragraph("Notas Adicionales")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10);
                document.Add(notesTitle);

                var notesContent = new Paragraph(vitalSign.Notes)
                    .SetFont(normalFont)
                    .SetMarginBottom(20);
                document.Add(notesContent);
            }

            // Pie de página
            var footer = new Paragraph($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {vitalSign.VitalSignId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"SignosVitales_{vitalSign.Consultation.MedicalRecord.Pet.PetName}_{vitalSign.RecordedDate:yyyyMMddHHmm}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // GET: VitalSigns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

            var viewModel = new VitalSignListViewModel
            {
                VitalSignId = vitalSign.VitalSignId,
                ConsultationId = vitalSign.ConsultationId,
                ConsultationInfo = $"Consulta del {vitalSign.Consultation.ConsultationDate:dd/MM/yyyy}",
                PetName = vitalSign.Consultation.MedicalRecord.Pet.PetName,
                RecordedDate = vitalSign.RecordedDate,
                Temperature = vitalSign.Temperature,
                HeartRate = vitalSign.HeartRate,
                RespiratoryRate = vitalSign.RespiratoryRate,
                Weight = vitalSign.Weight,
                BloodPressure = vitalSign.BloodPressureSystolic.HasValue && vitalSign.BloodPressureDiastolic.HasValue ?
                    $"{vitalSign.BloodPressureSystolic}/{vitalSign.BloodPressureDiastolic}" : "N/A"
            };

            ViewBag.Notes = vitalSign.Notes;
            return View(viewModel);
        }

        // GET: VitalSigns/Create
        public async Task<IActionResult> Create()
        {
            await LoadConsultationsViewData();
            return View(new VitalSignFormViewModel
            {
                RecordedDate = DateTime.Now
            });
        }

        // POST: VitalSigns/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VitalSignFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vitalSign = new VitalSign
                    {
                        ConsultationId = model.ConsultationId,
                        Temperature = model.Temperature,
                        HeartRate = model.HeartRate,
                        RespiratoryRate = model.RespiratoryRate,
                        Weight = model.Weight,
                        BloodPressureSystolic = model.BloodPressureSystolic,
                        BloodPressureDiastolic = model.BloodPressureDiastolic,
                        Notes = model.Notes,
                        RecordedDate = model.RecordedDate
                    };

                    _context.Add(vitalSign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Signos vitales registrados exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al registrar signos vitales");
                    ModelState.AddModelError("", "No se pudo registrar los signos vitales. Intente nuevamente.");
                }
            }

            await LoadConsultationsViewData();
            return View(model);
        }

        // GET: VitalSigns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                .FirstOrDefaultAsync(vs => vs.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

            var model = new VitalSignFormViewModel
            {
                VitalSignId = vitalSign.VitalSignId,
                ConsultationId = vitalSign.ConsultationId,
                Temperature = vitalSign.Temperature,
                HeartRate = vitalSign.HeartRate,
                RespiratoryRate = vitalSign.RespiratoryRate,
                Weight = vitalSign.Weight,
                BloodPressureSystolic = vitalSign.BloodPressureSystolic,
                BloodPressureDiastolic = vitalSign.BloodPressureDiastolic,
                Notes = vitalSign.Notes,
                RecordedDate = vitalSign.RecordedDate
            };

            ViewBag.ConsultationInfo = $"Consulta del {vitalSign.Consultation.ConsultationDate:dd/MM/yyyy}";
            await LoadConsultationsViewData();
            return View(model);
        }

        // POST: VitalSigns/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VitalSignFormViewModel model)
        {
            if (id != model.VitalSignId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var vitalSign = await _context.VitalSigns.FindAsync(id);
                    if (vitalSign == null)
                    {
                        return NotFound();
                    }

                    vitalSign.Temperature = model.Temperature;
                    vitalSign.HeartRate = model.HeartRate;
                    vitalSign.RespiratoryRate = model.RespiratoryRate;
                    vitalSign.Weight = model.Weight;
                    vitalSign.BloodPressureSystolic = model.BloodPressureSystolic;
                    vitalSign.BloodPressureDiastolic = model.BloodPressureDiastolic;
                    vitalSign.Notes = model.Notes;
                    vitalSign.RecordedDate = model.RecordedDate;

                    _context.Update(vitalSign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Signos vitales actualizados exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar signos vitales");
                    ModelState.AddModelError("", "No se pudo actualizar los signos vitales. Intente nuevamente.");
                }
            }

            await LoadConsultationsViewData();
            return View(model);
        }

        // GET: VitalSigns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .FirstOrDefaultAsync(m => m.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

            return View(vitalSign);
        }

        // POST: VitalSigns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vitalSign = await _context.VitalSigns.FindAsync(id);
            if (vitalSign != null)
            {
                _context.VitalSigns.Remove(vitalSign);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Signos vitales eliminados exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadConsultationsViewData()
        {
            ViewBag.Consultations = await _context.MedicalConsultations
                .Include(c => c.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .OrderByDescending(c => c.ConsultationDate)
                .Select(c => new
                {
                    ConsultationId = c.ConsultationId,
                    DisplayText = $"{c.MedicalRecord.Pet.PetName} - {c.ConsultationDate:dd/MM/yyyy HH:mm}"
                })
                .ToListAsync();
        }
    }
}
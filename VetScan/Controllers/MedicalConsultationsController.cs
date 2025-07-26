// Controllers/MedicalConsultationsController.cs
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
    public class MedicalConsultationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalConsultationsController> _logger;

        public MedicalConsultationsController(
            ApplicationDbContext context,
            ILogger<MedicalConsultationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.MedicalConsultations
                .Include(mc => mc.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .Include(mc => mc.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(mc => mc.ConsultationDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(mc =>
                    mc.MedicalRecord.Pet.PetName.Contains(searchString) ||
                    (mc.AttendingVeterinarian.User.FirstName + " " + mc.AttendingVeterinarian.User.LastName).Contains(searchString));
            }

            var consultations = await query
                .Select(mc => new MedicalConsultationListViewModel
                {
                    ConsultationId = mc.ConsultationId,
                    RecordNumber = mc.MedicalRecord.RecordNumber,
                    PetName = mc.MedicalRecord.Pet.PetName,
                    VeterinarianName = $"{mc.AttendingVeterinarian.User.FirstName} {mc.AttendingVeterinarian.User.LastName}",
                    ConsultationDate = mc.ConsultationDate,
                    ConsultationType = mc.ConsultationType,
                    Status = mc.Status
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
            var title = new Paragraph("Reporte de Consultas Médicas")
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
            var table = new Table(new float[] { 2, 2, 2, 2, 2, 2 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascota").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Registro").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Veterinario").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in consultations)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedDate).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.RecordNumber).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.VeterinarianName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.ConsultationType).SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(item.Status).SetFont(normalFont));
                statusCell.SetBackgroundColor(
                    item.Status == "Completed" ? new DeviceRgb(40, 167, 69) :
                    item.Status == "Pending" ? new DeviceRgb(255, 193, 7) :
                    new DeviceRgb(220, 53, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"ConsultasMedicas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.MedicalConsultations
                .Include(mc => mc.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .Include(mc => mc.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(mc => mc.ConsultationDate)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(mc =>
                    mc.MedicalRecord.Pet.PetName.Contains(searchString) ||
                    (mc.AttendingVeterinarian.User.FirstName + " " + mc.AttendingVeterinarian.User.LastName).Contains(searchString));
            }

            var consultations = await query
                .Select(mc => new MedicalConsultationListViewModel
                {
                    ConsultationId = mc.ConsultationId,
                    RecordNumber = mc.MedicalRecord.RecordNumber,
                    PetName = mc.MedicalRecord.Pet.PetName,
                    VeterinarianName = $"{mc.AttendingVeterinarian.User.FirstName} {mc.AttendingVeterinarian.User.LastName}",
                    ConsultationDate = mc.ConsultationDate,
                    ConsultationType = mc.ConsultationType,
                    Status = mc.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(consultations);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var consultation = await _context.MedicalConsultations
                .Include(mc => mc.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .Include(mc => mc.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(mc => mc.ConsultationId == id);

            if (consultation == null)
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
            var title = new Paragraph($"Consulta Médica - {consultation.ConsultationDate:dd/MM/yyyy}")
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
            infoTable.AddCell(CreateCell(consultation.ConsultationDate.ToString("dd/MM/yyyy HH:mm"), normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Mascota:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(consultation.MedicalRecord.Pet.PetName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("N° Registro:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(consultation.MedicalRecord.RecordNumber, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Veterinario:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(
                $"{consultation.AttendingVeterinarian.User.FirstName} {consultation.AttendingVeterinarian.User.LastName}",
                normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Tipo de Consulta:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(consultation.ConsultationType, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(consultation.Status, normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(
                consultation.Status == "Completed" ? new DeviceRgb(40, 167, 69) :
                consultation.Status == "Pending" ? new DeviceRgb(255, 193, 7) :
                new DeviceRgb(220, 53, 69));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            document.Add(infoTable);

            // Sección de Diagnóstico
            var diagnosisTitle = new Paragraph("Diagnóstico")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(diagnosisTitle);

            var diagnosisContent = new Paragraph(string.IsNullOrEmpty(consultation.Diagnosis) ?
                "No se registró diagnóstico" : consultation.Diagnosis)
                .SetFont(normalFont)
                .SetMarginBottom(20);
            document.Add(diagnosisContent);

            // Sección de Tratamiento
            var treatmentTitle = new Paragraph("Tratamiento")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(treatmentTitle);

            var treatmentContent = new Paragraph(string.IsNullOrEmpty(consultation.Treatment) ?
                "No se registró tratamiento" : consultation.Treatment)
                .SetFont(normalFont)
                .SetMarginBottom(20);
            document.Add(treatmentContent);

            // Próxima cita recomendada
            if (consultation.NextAppointmentRecommended.HasValue)
            {
                var nextAppointmentTitle = new Paragraph("Próxima Cita Recomendada")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetMarginBottom(10);
                document.Add(nextAppointmentTitle);

                var nextAppointmentContent = new Paragraph(
                    consultation.NextAppointmentRecommended.Value.ToString("dd/MM/yyyy HH:mm"))
                    .SetFont(normalFont)
                    .SetMarginBottom(20);
                document.Add(nextAppointmentContent);
            }

            // Pie de página
            var footer = new Paragraph($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {consultation.ConsultationId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Consulta_{consultation.MedicalRecord.Pet.PetName}_{consultation.ConsultationDate:yyyyMMdd}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // GET: MedicalConsultations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.MedicalConsultations
                .Include(mc => mc.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .Include(mc => mc.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(mc => mc.ConsultationId == id);

            if (consultation == null)
            {
                return NotFound();
            }

            var viewModel = new MedicalConsultationListViewModel
            {
                ConsultationId = consultation.ConsultationId,
                RecordNumber = consultation.MedicalRecord.RecordNumber,
                PetName = consultation.MedicalRecord.Pet.PetName,
                VeterinarianName = $"{consultation.AttendingVeterinarian.User.FirstName} {consultation.AttendingVeterinarian.User.LastName}",
                ConsultationDate = consultation.ConsultationDate,
                ConsultationType = consultation.ConsultationType,
                Status = consultation.Status
            };

            ViewBag.Diagnosis = consultation.Diagnosis;
            ViewBag.Treatment = consultation.Treatment;
            ViewBag.NextAppointment = consultation.NextAppointmentRecommended;

            return View(viewModel);
        }

        // GET: MedicalConsultations/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new MedicalConsultationFormViewModel
            {
                ConsultationDate = DateTime.Now
            });
        }

        // POST: MedicalConsultations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalConsultationFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var consultation = new MedicalConsultation
                    {
                        MedicalRecordId = model.MedicalRecordId,
                        VeterinarianId = model.VeterinarianId,
                        ConsultationDate = model.ConsultationDate,
                        ConsultationType = model.ConsultationType,
                        Diagnosis = model.Diagnosis,
                        Treatment = model.Treatment,
                        NextAppointmentRecommended = model.NextAppointmentRecommended,
                        Status = model.Status
                    };

                    _context.Add(consultation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Consulta médica creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear consulta médica");
                    ModelState.AddModelError("", "No se pudo crear la consulta médica. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: MedicalConsultations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var consultation = await _context.MedicalConsultations
                    .Include(mc => mc.MedicalRecord)
                    .Include(mc => mc.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(mc => mc.ConsultationId == id);

                if (consultation == null)
                {
                    _logger.LogWarning($"Consulta médica con ID {id} no encontrada");
                    return NotFound();
                }

                if (consultation.AttendingVeterinarian == null)
                {
                    _logger.LogWarning($"Consulta médica con ID {id} no tiene veterinario asociado");
                    return NotFound("Veterinario no encontrado");
                }

                if (consultation.AttendingVeterinarian.User == null)
                {
                    _logger.LogWarning($"Veterinario con ID {consultation.VeterinarianId} no tiene usuario asociado");
                    return NotFound("Usuario del veterinario no encontrado");
                }

                var model = new MedicalConsultationFormViewModel
                {
                    ConsultationId = consultation.ConsultationId,
                    MedicalRecordId = consultation.MedicalRecordId,
                    VeterinarianId = consultation.VeterinarianId,
                    ConsultationDate = consultation.ConsultationDate,
                    ConsultationType = consultation.ConsultationType,
                    Diagnosis = consultation.Diagnosis,
                    Treatment = consultation.Treatment,
                    NextAppointmentRecommended = consultation.NextAppointmentRecommended,
                    Status = consultation.Status
                };

                ViewBag.RecordNumber = consultation.MedicalRecord?.RecordNumber ?? "N/A";
                ViewBag.VeterinarianName = $"{consultation.AttendingVeterinarian.User.FirstName} {consultation.AttendingVeterinarian.User.LastName}";

                await LoadViewData();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar la consulta médica con ID {id} para edición");
                return StatusCode(500, "Error interno al cargar la consulta médica");
            }
        }

        // POST: MedicalConsultations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalConsultationFormViewModel model)
        {
            if (id != model.ConsultationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var consultation = await _context.MedicalConsultations.FindAsync(id);
                    if (consultation == null)
                    {
                        return NotFound();
                    }

                    consultation.ConsultationDate = model.ConsultationDate;
                    consultation.ConsultationType = model.ConsultationType;
                    consultation.Diagnosis = model.Diagnosis;
                    consultation.Treatment = model.Treatment;
                    consultation.NextAppointmentRecommended = model.NextAppointmentRecommended;
                    consultation.Status = model.Status;

                    _context.Update(consultation);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Consulta médica actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar consulta médica");
                    ModelState.AddModelError("", "No se pudo actualizar la consulta médica. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: MedicalConsultations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.MedicalConsultations
                .Include(mc => mc.MedicalRecord)
                .Include(mc => mc.AttendingVeterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(mc => mc.ConsultationId == id);

            if (consultation == null)
            {
                return NotFound();
            }

            return View(consultation);
        }

        // POST: MedicalConsultations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consultation = await _context.MedicalConsultations.FindAsync(id);
            if (consultation != null)
            {
                _context.MedicalConsultations.Remove(consultation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Consulta médica eliminada exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewData()
        {
            // Cargar registros médicos activos
            ViewBag.MedicalRecords = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .Where(mr => mr.Status == "Active")
                .Select(mr => new
                {
                    MedicalRecordId = mr.MedicalRecordId,
                    DisplayText = $"{mr.RecordNumber} - {mr.Pet.PetName}"
                })
                .ToListAsync();

            // Cargar veterinarios activos
            ViewBag.Veterinarians = await _context.Veterinarians
                .Include(v => v.User)
                .Where(v => v.User.Role.IsActive)
                .Select(v => new
                {
                    VeterinarianId = v.VeterinarianId,
                    DisplayText = $"{v.User.FirstName} {v.User.LastName}"
                })
                .ToListAsync();

            // Tipos de consulta predefinidos
            ViewBag.ConsultationTypes = new List<string>
            {
                "General",
                "Emergencia",
                "Cirugía",
                "Dermatología",
                "Odontología",
                "Oftalmología",
                "Cardiología",
                "Control"
            };

            // Estados posibles
            ViewBag.StatusOptions = new List<string>
            {
                "Scheduled",
                "Completed",
                "Cancelled",
                "No Show"
            };
        }
    }
}
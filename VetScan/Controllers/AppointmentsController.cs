// AppointmentsController.cs
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
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            ApplicationDbContext context,
            ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(a => a.AppointmentDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a =>
                    a.Pet.PetName.Contains(searchString) ||
                    (a.Veterinarian.User.FirstName + " " + a.Veterinarian.User.LastName).Contains(searchString));
            }

            var appointments = await query
                .Select(a => new AppointmentListViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PetName = a.Pet.PetName,
                    VeterinarianName = $"{a.Veterinarian.User.FirstName} {a.Veterinarian.User.LastName}",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentType = a.AppointmentType,
                    Status = a.Status
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
            var title = new Paragraph("Reporte de Citas Programadas")
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
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha y Hora").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascota").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Veterinario").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in appointments)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.FormattedDate).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.VeterinarianName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.AppointmentType).SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(item.Status).SetFont(normalFont));
                statusCell.SetBackgroundColor(
                    item.Status == "Completed" ? new DeviceRgb(25, 135, 84) :
                    item.Status == "Cancelled" ? new DeviceRgb(220, 53, 69) :
                    new DeviceRgb(13, 110, 253));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"CitasProgramadas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // Obtener los datos (igual que en el Index y ExportToPdf)
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(a => a.AppointmentDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a =>
                    a.Pet.PetName.Contains(searchString) ||
                    (a.Veterinarian.User.FirstName + " " + a.Veterinarian.User.LastName).Contains(searchString));
            }

            var appointments = await query
                .Select(a => new AppointmentListViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PetName = a.Pet.PetName,
                    VeterinarianName = $"{a.Veterinarian.User.FirstName} {a.Veterinarian.User.LastName}",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentType = a.AppointmentType,
                    Status = a.Status
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Citas Programadas");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Fecha y Hora";
                worksheet.Cell(currentRow, 2).Value = "Mascota";
                worksheet.Cell(currentRow, 3).Value = "Veterinario";
                worksheet.Cell(currentRow, 4).Value = "Tipo";
                worksheet.Cell(currentRow, 5).Value = "Estado";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 5);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in appointments)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.FormattedDate;
                    worksheet.Cell(currentRow, 2).Value = item.PetName;
                    worksheet.Cell(currentRow, 3).Value = item.VeterinarianName;
                    worksheet.Cell(currentRow, 4).Value = item.AppointmentType;
                    worksheet.Cell(currentRow, 5).Value = item.Status;

                    // Color para estado
                    var statusCell = worksheet.Cell(currentRow, 5);
                    statusCell.Style.Fill.BackgroundColor =
                        item.Status == "Completed" ? XLColor.Green :
                        item.Status == "Cancelled" ? XLColor.Red :
                        XLColor.Blue;
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
                        $"CitasProgramadas_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        // GET: Appointments
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(a => a.AppointmentDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a =>
                    a.Pet.PetName.Contains(searchString) ||
                    (a.Veterinarian.User.FirstName + " " + a.Veterinarian.User.LastName).Contains(searchString));
            }

            var appointments = await query
                .Select(a => new AppointmentListViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PetName = a.Pet.PetName,
                    VeterinarianName = $"{a.Veterinarian.User.FirstName} {a.Veterinarian.User.LastName}",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentType = a.AppointmentType,
                    Status = a.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(appointments);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
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
            var title = new Paragraph("COMPROBANTE DE CITA VETERINARIA")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Subtítulo
            var subtitle = new Paragraph($"N° {appointment.AppointmentId}")
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
            infoTable.AddCell(CreateCell(appointment.Pet.PetName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Veterinario:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(
                $"{appointment.Veterinarian.User.FirstName} {appointment.Veterinarian.User.LastName}",
                normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Fecha y Hora:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"), normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Duración:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell($"{appointment.Duration} minutos", normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Detalles de la cita
            var detailsTitle = new Paragraph("DETALLES DE LA CITA")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(detailsTitle);

            var detailsTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            detailsTable.AddCell(CreateCell("Tipo:", boldFont, TextAlignment.LEFT));
            detailsTable.AddCell(CreateCell(appointment.AppointmentType, normalFont, TextAlignment.LEFT));

            detailsTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(appointment.Status, normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(
                appointment.Status == "Completed" ? new DeviceRgb(25, 135, 84) :
                appointment.Status == "Cancelled" ? new DeviceRgb(220, 53, 69) :
                new DeviceRgb(13, 110, 253));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            detailsTable.AddCell(statusCell);

            detailsTable.AddCell(CreateCell("Costo Estimado:", boldFont, TextAlignment.LEFT));
            detailsTable.AddCell(CreateCell(
                appointment.EstimatedCost?.ToString("C") ?? "No especificado",
                normalFont, TextAlignment.LEFT));

            detailsTable.AddCell(CreateCell("Costo Real:", boldFont, TextAlignment.LEFT));
            detailsTable.AddCell(CreateCell(
                appointment.ActualCost?.ToString("C") ?? "No especificado",
                normalFont, TextAlignment.LEFT));

            document.Add(detailsTable);

            // Razón de la visita
            if (!string.IsNullOrEmpty(appointment.ReasonForVisit))
            {
                var reasonTitle = new Paragraph("RAZÓN DE LA VISITA")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(reasonTitle);

                var reasonContent = new Paragraph(appointment.ReasonForVisit)
                    .SetFont(normalFont)
                    .SetMarginBottom(20)
                    .SetPaddingLeft(20)
                    .SetPaddingRight(20);
                document.Add(reasonContent);
            }

            // Notas adicionales
            if (!string.IsNullOrEmpty(appointment.Notes))
            {
                var notesTitle = new Paragraph("NOTAS ADICIONALES")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(notesTitle);

                var notesContent = new Paragraph(appointment.Notes)
                    .SetFont(normalFont)
                    .SetMarginBottom(20)
                    .SetPaddingLeft(20)
                    .SetPaddingRight(20);
                document.Add(notesContent);
            }

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {appointment.AppointmentId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Cita_{appointment.Pet.PetName}_{appointment.AppointmentDate:yyyyMMddHHmm}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var viewModel = new AppointmentListViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PetName = appointment.Pet.PetName,
                VeterinarianName = $"{appointment.Veterinarian.User.FirstName} {appointment.Veterinarian.User.LastName}",
                AppointmentDate = appointment.AppointmentDate,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status
            };

            ViewBag.Duration = appointment.Duration;
            ViewBag.Notes = appointment.Notes;
            ViewBag.ReasonForVisit = appointment.ReasonForVisit;
            ViewBag.EstimatedCost = appointment.EstimatedCost?.ToString("C");
            ViewBag.ActualCost = appointment.ActualCost?.ToString("C");

            return View(viewModel);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new AppointmentFormViewModel
            {
                AppointmentDate = DateTime.Now
            });
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = new Appointment
                    {
                        PetId = model.PetId,
                        VeterinarianId = model.VeterinarianId,
                        AppointmentDate = model.AppointmentDate,
                        Duration = model.Duration,
                        AppointmentType = model.AppointmentType,
                        Status = model.Status,
                        Notes = model.Notes,
                        ReasonForVisit = model.ReasonForVisit,
                        EstimatedCost = model.EstimatedCost,
                        ActualCost = model.ActualCost
                    };

                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cita creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear cita");
                    ModelState.AddModelError("", "No se pudo crear la cita. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var model = new AppointmentFormViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PetId = appointment.PetId,
                VeterinarianId = appointment.VeterinarianId,
                AppointmentDate = appointment.AppointmentDate,
                Duration = appointment.Duration,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status,
                Notes = appointment.Notes,
                ReasonForVisit = appointment.ReasonForVisit,
                EstimatedCost = appointment.EstimatedCost,
                ActualCost = appointment.ActualCost
            };

            await LoadViewData();
            ViewBag.PetName = (await _context.Pets.FindAsync(appointment.PetId))?.PetName;
            ViewBag.VeterinarianName = (await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VeterinarianId == appointment.VeterinarianId))?.User.FirstName;

            return View(model);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentFormViewModel model)
        {
            if (id != model.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    appointment.AppointmentDate = model.AppointmentDate;
                    appointment.Duration = model.Duration;
                    appointment.AppointmentType = model.AppointmentType;
                    appointment.Status = model.Status;
                    appointment.Notes = model.Notes;
                    appointment.ReasonForVisit = model.ReasonForVisit;
                    appointment.EstimatedCost = model.EstimatedCost;
                    appointment.ActualCost = model.ActualCost;

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cita actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar cita");
                    ModelState.AddModelError("", "No se pudo actualizar la cita. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cita eliminada exitosamente";
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

            // Tipos de cita predefinidos
            ViewBag.AppointmentTypes = new List<string>
            {
                "Consulta General",
                "Vacunación",
                "Cirugía",
                "Control",
                "Emergencia",
                "Limpieza Dental",
                "Dermatología",
                "Oftalmología"
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
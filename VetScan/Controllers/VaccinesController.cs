// Controllers/VaccinesController.cs
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
    public class VaccinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccinesController> _logger;

        public VaccinesController(ApplicationDbContext context, ILogger<VaccinesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Vaccines
                .Include(v => v.Species)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.VaccineName.Contains(searchString) ||
                    (v.Manufacturer != null && v.Manufacturer.Contains(searchString)) ||
                    (v.VaccineType != null && v.VaccineType.Contains(searchString)) ||
                    (v.Species != null && v.Species.SpeciesName.Contains(searchString)));
            }

            var vaccines = await query
                .OrderBy(v => v.VaccineName)
                .Select(v => new VaccineListViewModel
                {
                    VaccineId = v.VaccineId,
                    VaccineName = v.VaccineName,
                    Manufacturer = v.Manufacturer,
                    VaccineType = v.VaccineType,
                    SpeciesName = v.Species != null ? v.Species.SpeciesName : "Todas",
                    IsCore = v.IsCore,
                    IsActive = v.IsActive
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Vacunas");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Nombre";
                worksheet.Cell(currentRow, 2).Value = "Fabricante";
                worksheet.Cell(currentRow, 3).Value = "Tipo";
                worksheet.Cell(currentRow, 4).Value = "Especie";
                worksheet.Cell(currentRow, 5).Value = "Tipo";
                worksheet.Cell(currentRow, 6).Value = "Estado";
                worksheet.Cell(currentRow, 7).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in vaccines)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.VaccineName;
                    worksheet.Cell(currentRow, 2).Value = item.Manufacturer ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = item.VaccineType ?? "N/A";
                    worksheet.Cell(currentRow, 4).Value = item.SpeciesName;
                    worksheet.Cell(currentRow, 5).Value = item.IsCore ? "Básica" : "Opcional";
                    worksheet.Cell(currentRow, 6).Value = item.IsActive ? "Activa" : "Inactiva";
                    worksheet.Cell(currentRow, 7).Value = item.VaccineId;

                    // Color para tipo y estado
                    var typeCell = worksheet.Cell(currentRow, 5);
                    typeCell.Style.Fill.BackgroundColor = item.IsCore ?
                        XLColor.Blue : XLColor.Gray;
                    typeCell.Style.Font.FontColor = XLColor.White;

                    var statusCell = worksheet.Cell(currentRow, 6);
                    statusCell.Style.Fill.BackgroundColor = item.IsActive ?
                        XLColor.Green : XLColor.Red;
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
                        $"Vacunas_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Vaccines
                .Include(v => v.Species)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.VaccineName.Contains(searchString) ||
                    (v.Manufacturer != null && v.Manufacturer.Contains(searchString)) ||
                    (v.VaccineType != null && v.VaccineType.Contains(searchString)) ||
                    (v.Species != null && v.Species.SpeciesName.Contains(searchString)));
            }

            var vaccines = await query
                .OrderBy(v => v.VaccineName)
                .Select(v => new VaccineListViewModel
                {
                    VaccineId = v.VaccineId,
                    VaccineName = v.VaccineName,
                    Manufacturer = v.Manufacturer,
                    VaccineType = v.VaccineType,
                    SpeciesName = v.Species != null ? v.Species.SpeciesName : "Todas",
                    IsCore = v.IsCore,
                    IsActive = v.IsActive,
                    CreatedDate = v.CreatedDate
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
            var title = new Paragraph("Reporte de Vacunas")
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
            var table = new Table(new float[] { 2, 2, 2, 2, 1, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fabricante").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Especie").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in vaccines)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.VaccineName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Manufacturer ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.VaccineType ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.SpeciesName).SetFont(normalFont)));

                var typeCell = new Cell().Add(new Paragraph(item.IsCore ? "Básica" : "Opcional").SetFont(normalFont));
                typeCell.SetBackgroundColor(item.IsCore ? new DeviceRgb(13, 110, 253) : new DeviceRgb(108, 117, 125));
                typeCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(typeCell);

                var statusCell = new Cell().Add(new Paragraph(item.IsActive ? "Activa" : "Inactiva").SetFont(normalFont));
                statusCell.SetBackgroundColor(item.IsActive ? new DeviceRgb(25, 135, 84) : new DeviceRgb(220, 53, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Vacunas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: Vaccines
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base como IQueryable
            var query = _context.Vaccines
                .Include(v => v.Species)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.VaccineName.Contains(searchString) ||
                    (v.Manufacturer != null && v.Manufacturer.Contains(searchString)) ||
                    (v.VaccineType != null && v.VaccineType.Contains(searchString)) ||
                    (v.Species != null && v.Species.SpeciesName.Contains(searchString)));
            }

            var vaccines = await query
                .OrderBy(v => v.VaccineName)
                .Select(v => new VaccineListViewModel
                {
                    VaccineId = v.VaccineId,
                    VaccineName = v.VaccineName,
                    Manufacturer = v.Manufacturer,
                    VaccineType = v.VaccineType,
                    SpeciesName = v.Species != null ? v.Species.SpeciesName : "Todas",
                    IsCore = v.IsCore,
                    IsActive = v.IsActive,
                    CreatedDate = v.CreatedDate
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(vaccines);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var vaccine = await _context.Vaccines
                .Include(v => v.Species)
                .FirstOrDefaultAsync(v => v.VaccineId == id);

            if (vaccine == null)
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
            var title = new Paragraph($"FICHA TÉCNICA DE VACUNA")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Subtítulo
            var subtitle = new Paragraph(vaccine.VaccineName)
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(subtitle);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Nombre:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccine.VaccineName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Fabricante:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccine.Manufacturer ?? "No especificado", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Tipo de vacuna:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccine.VaccineType ?? "No especificado", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Especie:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(vaccine.Species?.SpeciesName ?? "Todas las especies", normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Características de la vacuna
            var specsTitle = new Paragraph("CARACTERÍSTICAS")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(specsTitle);

            var specsTable = new Table(new float[] { 4, 6 })
                .SetWidth(UnitValue.CreatePercentValue(80))
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .SetMarginBottom(20);

            specsTable.AddCell(CreateCell("Tipo:", boldFont, TextAlignment.LEFT));
            var typeCell = CreateCell(vaccine.IsCore ? "Vacuna básica" : "Vacuna opcional", normalFont, TextAlignment.LEFT);
            typeCell.SetBackgroundColor(vaccine.IsCore ? new DeviceRgb(13, 110, 253) : new DeviceRgb(108, 117, 125));
            typeCell.SetFontColor(DeviceRgb.WHITE);
            specsTable.AddCell(typeCell);

            specsTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(vaccine.IsActive ? "Activa" : "Inactiva", normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(vaccine.IsActive ? new DeviceRgb(25, 135, 84) : new DeviceRgb(220, 53, 69));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            specsTable.AddCell(statusCell);

            specsTable.AddCell(CreateCell("Edad recomendada:", boldFont, TextAlignment.LEFT));
            specsTable.AddCell(CreateCell(vaccine.RecommendedAge ?? "No especificada", normalFont, TextAlignment.LEFT));

            specsTable.AddCell(CreateCell("Intervalo de refuerzo:", boldFont, TextAlignment.LEFT));
            specsTable.AddCell(CreateCell(
                vaccine.BoosterInterval.HasValue ? $"{vaccine.BoosterInterval} días" : "No especificado",
                normalFont, TextAlignment.LEFT));

            document.Add(specsTable);

            // Información adicional
            var additionalInfo = new Paragraph("INFORMACIÓN ADICIONAL")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(additionalInfo);

            var infoList = new List()
                .SetListSymbol(ListNumberingType.DECIMAL)
                .SetMarginBottom(20);

            infoList.Add(new ListItem("Fecha de creación: " + vaccine.CreatedDate.ToString("dd/MM/yyyy HH:mm")));

            if (vaccine.Species != null)
            {
                infoList.Add(new ListItem($"Especie objetivo: {vaccine.Species.SpeciesName}"));
            }
            else
            {
                infoList.Add(new ListItem("Aplicable a todas las especies"));
            }

            document.Add(infoList);

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {vaccine.VaccineId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Vacuna_{vaccine.VaccineName}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // GET: Vaccines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccine = await _context.Vaccines
                .Include(v => v.Species)
                .FirstOrDefaultAsync(m => m.VaccineId == id);

            if (vaccine == null)
            {
                return NotFound();
            }

            var viewModel = new VaccineListViewModel
            {
                VaccineId = vaccine.VaccineId,
                VaccineName = vaccine.VaccineName,
                Manufacturer = vaccine.Manufacturer,
                VaccineType = vaccine.VaccineType,
                SpeciesName = vaccine.Species != null ? vaccine.Species.SpeciesName : "Todas",
                IsCore = vaccine.IsCore,
                IsActive = vaccine.IsActive,
                CreatedDate = vaccine.CreatedDate
            };

            ViewBag.RecommendedAge = vaccine.RecommendedAge;
            ViewBag.BoosterInterval = vaccine.BoosterInterval;
            ViewBag.CreatedDate = vaccine.CreatedDate.ToString("dd/MM/yyyy HH:mm");

            return View(viewModel);
        }

        // GET: Vaccines/Create
        public async Task<IActionResult> Create()
        {
            await LoadSpeciesViewData();
            return View(new VaccineFormViewModel
            {
                IsActive = true
            });
        }

        // POST: Vaccines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VaccineFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vaccine = new Vaccine
                    {
                        VaccineName = model.VaccineName,
                        Manufacturer = model.Manufacturer,
                        VaccineType = model.VaccineType,
                        SpeciesId = model.SpeciesId,
                        RecommendedAge = model.RecommendedAge,
                        BoosterInterval = model.BoosterInterval,
                        IsCore = model.IsCore,
                        IsActive = model.IsActive,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(vaccine);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vacuna creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear vacuna");
                    ModelState.AddModelError("", "No se pudo crear la vacuna. Intente nuevamente.");
                }
            }

            await LoadSpeciesViewData();
            return View(model);
        }

        // GET: Vaccines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccine = await _context.Vaccines.FindAsync(id);
            if (vaccine == null)
            {
                return NotFound();
            }

            var model = new VaccineFormViewModel
            {
                VaccineId = vaccine.VaccineId,
                VaccineName = vaccine.VaccineName,
                Manufacturer = vaccine.Manufacturer,
                VaccineType = vaccine.VaccineType,
                SpeciesId = vaccine.SpeciesId,
                RecommendedAge = vaccine.RecommendedAge,
                BoosterInterval = vaccine.BoosterInterval,
                IsCore = vaccine.IsCore,
                IsActive = vaccine.IsActive
            };

            await LoadSpeciesViewData();
            return View(model);
        }

        // POST: Vaccines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VaccineFormViewModel model)
        {
            if (id != model.VaccineId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var vaccine = await _context.Vaccines.FindAsync(id);
                    if (vaccine == null)
                    {
                        return NotFound();
                    }

                    vaccine.VaccineName = model.VaccineName;
                    vaccine.Manufacturer = model.Manufacturer;
                    vaccine.VaccineType = model.VaccineType;
                    vaccine.SpeciesId = model.SpeciesId;
                    vaccine.RecommendedAge = model.RecommendedAge;
                    vaccine.BoosterInterval = model.BoosterInterval;
                    vaccine.IsCore = model.IsCore;
                    vaccine.IsActive = model.IsActive;

                    _context.Update(vaccine);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vacuna actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar vacuna");
                    ModelState.AddModelError("", "No se pudo actualizar la vacuna. Intente nuevamente.");
                }
            }

            await LoadSpeciesViewData();
            return View(model);
        }

        // POST: Vaccines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vaccine = await _context.Vaccines.FindAsync(id);
            if (vaccine != null)
            {
                vaccine.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vacuna desactivada exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSpeciesViewData()
        {
            ViewBag.Species = await _context.AnimalSpecies
                .Where(s => s.IsActive)
                .OrderBy(s => s.SpeciesName)
                .Select(s => new
                {
                    SpeciesId = s.SpeciesId,
                    DisplayText = s.SpeciesName
                })
                .ToListAsync();

            ViewBag.VaccineTypes = new List<string>
            {
                "Virus Vivo",
                "Virus Muerto",
                "Toxoide",
                "Recombinante",
                "Subunidad",
                "Conjugada"
            };
        }
    }
}
// BreedsController.cs
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
    public class BreedsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BreedsController> _logger;

        public BreedsController(ApplicationDbContext context, ILogger<BreedsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Breeds
                .Include(b => b.Species)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b =>
                    b.BreedName.Contains(searchString) ||
                    (b.Description != null && b.Description.Contains(searchString)) ||
                    b.Species.SpeciesName.Contains(searchString));
            }

            var breeds = await query
                .OrderBy(b => b.Species.SpeciesName)
                .ThenBy(b => b.BreedName)
                .Select(b => new BreedListViewModel
                {
                    BreedId = b.BreedId,
                    BreedName = b.BreedName,
                    SpeciesName = b.Species.SpeciesName,
                    Description = b.Description,
                    IsActive = b.IsActive
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
            var title = new Paragraph("Reporte de Razas de Animales")
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
            var table = new Table(new float[] { 2, 2, 2, 1, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Especie").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in breeds)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.BreedName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.SpeciesName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Description ?? "N/A").SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(item.IsActive ? "Activa" : "Inactiva").SetFont(normalFont));
                statusCell.SetBackgroundColor(item.IsActive ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);

                table.AddCell(new Cell().Add(new Paragraph(item.BreedId.ToString()).SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Razas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: Breeds
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Breeds
                .Include(b => b.Species)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b =>
                    b.BreedName.Contains(searchString) ||
                    (b.Description != null && b.Description.Contains(searchString)) ||
                    b.Species.SpeciesName.Contains(searchString));
            }

            var breeds = await query
                .OrderBy(b => b.Species.SpeciesName)
                .ThenBy(b => b.BreedName)
                .Select(b => new BreedListViewModel
                {
                    BreedId = b.BreedId,
                    BreedName = b.BreedName,
                    SpeciesName = b.Species.SpeciesName,
                    Description = b.Description,
                    IsActive = b.IsActive
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(breeds);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var breed = await _context.Breeds
                .Include(b => b.Species)
                .FirstOrDefaultAsync(b => b.BreedId == id);

            if (breed == null)
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
            var title = new Paragraph("FICHA TÉCNICA DE RAZA")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Subtítulo
            var subtitle = new Paragraph(breed.BreedName)
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
            infoTable.AddCell(CreateCell(breed.BreedName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Especie:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(breed.Species.SpeciesName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(breed.IsActive ? "Activa" : "Inactiva", normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(breed.IsActive ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            document.Add(infoTable);

            // Descripción
            var descriptionTitle = new Paragraph("DESCRIPCIÓN")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(descriptionTitle);

            var descriptionContent = new Paragraph(string.IsNullOrEmpty(breed.Description) ?
                "No hay descripción disponible para esta raza." : breed.Description)
                .SetFont(normalFont)
                .SetMarginBottom(20)
                .SetPaddingLeft(20)
                .SetPaddingRight(20);
            document.Add(descriptionContent);

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

            infoList.Add(new ListItem($"ID de la raza: {breed.BreedId}"));
            infoList.Add(new ListItem($"ID de la especie: {breed.SpeciesId}"));
            infoList.Add(new ListItem($"Registro creado: {DateTime.Now.ToString("dd/MM/yyyy")}"));

            document.Add(infoList);

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Raza_{breed.BreedName}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // GET: Breeds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var breed = await _context.Breeds
                .Include(b => b.Species)
                .FirstOrDefaultAsync(m => m.BreedId == id);

            if (breed == null)
            {
                return NotFound();
            }

            var viewModel = new BreedListViewModel
            {
                BreedId = breed.BreedId,
                BreedName = breed.BreedName,
                SpeciesName = breed.Species.SpeciesName,
                Description = breed.Description,
                IsActive = breed.IsActive
            };

            return View(viewModel);
        }

        // GET: Breeds/Create
        public async Task<IActionResult> Create()
        {
            await LoadSpeciesViewData();
            return View();
        }

        // POST: Breeds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BreedFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var breed = new Breed
                    {
                        SpeciesId = model.SpeciesId,
                        BreedName = model.BreedName,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    _context.Add(breed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Raza creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear raza");
                    ModelState.AddModelError("", "No se pudo crear la raza. Intente nuevamente.");
                }
            }

            await LoadSpeciesViewData();
            return View(model);
        }

        // GET: Breeds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var breed = await _context.Breeds.FindAsync(id);
            if (breed == null)
            {
                return NotFound();
            }

            var model = new BreedFormViewModel
            {
                BreedId = breed.BreedId,
                SpeciesId = breed.SpeciesId,
                BreedName = breed.BreedName,
                Description = breed.Description,
                IsActive = breed.IsActive
            };

            await LoadSpeciesViewData();
            ViewBag.SpeciesName = (await _context.AnimalSpecies.FindAsync(breed.SpeciesId))?.SpeciesName;
            return View(model);
        }

        // POST: Breeds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BreedFormViewModel model)
        {
            if (id != model.BreedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var breed = await _context.Breeds.FindAsync(id);
                    if (breed == null)
                    {
                        return NotFound();
                    }

                    breed.SpeciesId = model.SpeciesId;
                    breed.BreedName = model.BreedName;
                    breed.Description = model.Description;
                    breed.IsActive = model.IsActive;

                    _context.Update(breed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Raza actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar raza");
                    ModelState.AddModelError("", "No se pudo actualizar la raza. Intente nuevamente.");
                }
            }

            await LoadSpeciesViewData();
            return View(model);
        }

        // GET: Breeds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var breed = await _context.Breeds
                .Include(b => b.Species)
                .FirstOrDefaultAsync(m => m.BreedId == id);

            if (breed == null)
            {
                return NotFound();
            }

            return View(breed);
        }

        // POST: Breeds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var breed = await _context.Breeds.FindAsync(id);
            if (breed != null)
            {
                breed.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Raza desactivada exitosamente";
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
        }
    }
}
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
    public class AnimalSpeciesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnimalSpeciesController> _logger;

        public AnimalSpeciesController(ApplicationDbContext context, ILogger<AnimalSpeciesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.AnimalSpecies.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.SpeciesName.Contains(searchString) ||
                    (s.Description != null && s.Description.Contains(searchString)));
            }

            var species = await query
                .OrderBy(s => s.SpeciesName)
                .Select(s => new AnimalSpeciesListViewModel
                {
                    SpeciesId = s.SpeciesId,
                    SpeciesName = s.SpeciesName,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    BreedCount = s.Breeds.Count(b => b.IsActive),
                    PetCount = s.Pets.Count(p => p.IsActive)
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Especies");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Nombre";
                worksheet.Cell(currentRow, 2).Value = "Descripción";
                worksheet.Cell(currentRow, 3).Value = "Razas";
                worksheet.Cell(currentRow, 4).Value = "Mascotas";
                worksheet.Cell(currentRow, 5).Value = "Estado";
                worksheet.Cell(currentRow, 6).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 6);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in species)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.SpeciesName;
                    worksheet.Cell(currentRow, 2).Value = item.Description ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = item.BreedCount;
                    worksheet.Cell(currentRow, 4).Value = item.PetCount;
                    worksheet.Cell(currentRow, 5).Value = item.IsActive ? "Activa" : "Inactiva";
                    worksheet.Cell(currentRow, 6).Value = item.SpeciesId;

                    // Color para estado
                    var statusCell = worksheet.Cell(currentRow, 5);
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
                        $"EspeciesAnimales_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.AnimalSpecies.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.SpeciesName.Contains(searchString) ||
                    (s.Description != null && s.Description.Contains(searchString)));
            }

            var species = await query
                .OrderBy(s => s.SpeciesName)
                .Select(s => new AnimalSpeciesListViewModel
                {
                    SpeciesId = s.SpeciesId,
                    SpeciesName = s.SpeciesName,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    BreedCount = s.Breeds.Count(b => b.IsActive),
                    PetCount = s.Pets.Count(p => p.IsActive)
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
            var title = new Paragraph("Reporte de Especies Animales")
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
            var table = new Table(new float[] { 2, 3, 1, 1, 1, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Razas").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mascotas").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in species)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.SpeciesName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Description ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.BreedCount.ToString()).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.PetCount.ToString()).SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(item.IsActive ? "Activa" : "Inactiva").SetFont(normalFont));
                statusCell.SetBackgroundColor(item.IsActive ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);

                table.AddCell(new Cell().Add(new Paragraph(item.SpeciesId.ToString()).SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"EspeciesAnimales_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: AnimalSpecies
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.AnimalSpecies
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.SpeciesName.Contains(searchString) ||
                    (s.Description != null && s.Description.Contains(searchString)));
            }

            var species = await query
                .OrderBy(s => s.SpeciesName)
                .Select(s => new AnimalSpeciesListViewModel
                {
                    SpeciesId = s.SpeciesId,
                    SpeciesName = s.SpeciesName,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    BreedCount = s.Breeds.Count(b => b.IsActive),
                    PetCount = s.Pets.Count(p => p.IsActive)
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(species);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var species = await _context.AnimalSpecies
                .Include(s => s.Breeds.Where(b => b.IsActive))
                .Include(s => s.Pets.Where(p => p.IsActive))
                .FirstOrDefaultAsync(s => s.SpeciesId == id);

            if (species == null) return NotFound();

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
            var title = new Paragraph("FICHA TÉCNICA DE ESPECIE ANIMAL")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Nombre:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(species.SpeciesName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(species.IsActive ? "Activa" : "Inactiva", normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(species.IsActive ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            infoTable.AddCell(CreateCell("Razas asociadas:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(species.Breeds.Count.ToString(), normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Mascotas registradas:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(species.Pets.Count.ToString(), normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Descripción
            var descriptionTitle = new Paragraph("DESCRIPCIÓN")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(descriptionTitle);

            var descriptionContent = new Paragraph(string.IsNullOrEmpty(species.Description) ?
                "No hay descripción disponible para esta especie." : species.Description)
                .SetFont(normalFont)
                .SetMarginBottom(20)
                .SetPaddingLeft(20)
                .SetPaddingRight(20);
            document.Add(descriptionContent);

            // Lista de razas
            if (species.Breeds.Any())
            {
                var breedsTitle = new Paragraph("RAZAS ASOCIADAS")
                    .SetFont(headerFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10);
                document.Add(breedsTitle);

                var breedsList = new List()
                    .SetListSymbol(ListNumberingType.DECIMAL)
                    .SetMarginBottom(20);

                foreach (var breed in species.Breeds.OrderBy(b => b.BreedName))
                {
                    breedsList.Add(new ListItem(breed.BreedName));
                }

                document.Add(breedsList);
            }

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {species.SpeciesId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Especie_{species.SpeciesName}_{DateTime.Now:yyyyMMdd}.pdf");
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
            var species = await _context.AnimalSpecies
                .Include(s => s.Breeds)
                .Include(s => s.Pets)
                .FirstOrDefaultAsync(s => s.SpeciesId == id);

            if (species == null) return NotFound();

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/AnimalSpecies/Details/{id}?exporting=true";

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
                $"Especie_{species.SpeciesName}_{DateTime.Now:yyyyMMdd}.{fileExtension}");
        }

        // GET: AnimalSpecies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var species = await _context.AnimalSpecies
                .Include(s => s.Breeds)
                .Include(s => s.Pets)
                .FirstOrDefaultAsync(m => m.SpeciesId == id);

            if (species == null) return NotFound();

            var viewModel = new AnimalSpeciesListViewModel
            {
                SpeciesId = species.SpeciesId,
                SpeciesName = species.SpeciesName,
                Description = species.Description,
                IsActive = species.IsActive,
                BreedCount = species.Breeds.Count(b => b.IsActive),
                PetCount = species.Pets.Count(p => p.IsActive)
            };

            return View(viewModel);
        }

        // GET: AnimalSpecies/Create
        public IActionResult Create() => View();

        // POST: AnimalSpecies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnimalSpeciesFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var species = new AnimalSpecies
                    {
                        SpeciesName = model.SpeciesName,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    _context.Add(species);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Especie creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear especie");
                    ModelState.AddModelError("", "No se pudo crear la especie. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: AnimalSpecies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var species = await _context.AnimalSpecies.FindAsync(id);
            if (species == null) return NotFound();

            var model = new AnimalSpeciesFormViewModel
            {
                SpeciesId = species.SpeciesId,
                SpeciesName = species.SpeciesName,
                Description = species.Description,
                IsActive = species.IsActive
            };

            return View(model);
        }

        // POST: AnimalSpecies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnimalSpeciesFormViewModel model)
        {
            if (id != model.SpeciesId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var species = await _context.AnimalSpecies.FindAsync(id);
                    if (species == null) return NotFound();

                    species.SpeciesName = model.SpeciesName;
                    species.Description = model.Description;
                    species.IsActive = model.IsActive;

                    _context.Update(species);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Especie actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar especie");
                    ModelState.AddModelError("", "No se pudo actualizar la especie. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: AnimalSpecies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var species = await _context.AnimalSpecies
                .Include(s => s.Breeds)
                .Include(s => s.Pets)
                .FirstOrDefaultAsync(m => m.SpeciesId == id);

            if (species == null) return NotFound();

            return View(species);
        }

        // POST: AnimalSpecies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var species = await _context.AnimalSpecies.FindAsync(id);
            if (species != null)
            {
                try
                {
                    species.IsActive = false; // Soft delete
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Especie desactivada exitosamente";
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al desactivar especie");
                    TempData["ErrorMessage"] = "No se pudo desactivar la especie porque tiene razas o mascotas asociadas.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
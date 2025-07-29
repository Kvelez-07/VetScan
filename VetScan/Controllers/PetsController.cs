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
    public class PetsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PetsController> _logger;

        public PetsController(ApplicationDbContext context, ILogger<PetsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            var query = _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.PetName.Contains(searchString) ||
                    p.PetCode.Contains(searchString));
            }

            var pets = await query
                .Select(p => new PetListViewModel
                {
                    PetId = p.PetId,
                    PetCode = p.PetCode,
                    PetName = p.PetName,
                    Species = p.Species.SpeciesName,
                    Breed = p.Breed != null ? p.Breed.BreedName : null,
                    OwnerName = $"{p.PetOwner.User.FirstName} {p.PetOwner.User.LastName}",
                    DateOfBirth = p.DateOfBirth,
                    GenderDisplay = p.Gender == "M" ? "Macho" : p.Gender == "F" ? "Hembra" : null,
                    Weight = p.Weight,
                    Color = p.Color
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Mascotas");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Código";
                worksheet.Cell(currentRow, 2).Value = "Nombre";
                worksheet.Cell(currentRow, 3).Value = "Especie";
                worksheet.Cell(currentRow, 4).Value = "Raza";
                worksheet.Cell(currentRow, 5).Value = "Dueño";
                worksheet.Cell(currentRow, 6).Value = "Fecha Nac.";
                worksheet.Cell(currentRow, 7).Value = "Edad";
                worksheet.Cell(currentRow, 8).Value = "Género";
                worksheet.Cell(currentRow, 9).Value = "Peso (kg)";
                worksheet.Cell(currentRow, 10).Value = "Color";
                worksheet.Cell(currentRow, 11).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 11);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in pets)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.PetCode;
                    worksheet.Cell(currentRow, 2).Value = item.PetName;
                    worksheet.Cell(currentRow, 3).Value = item.Species;
                    worksheet.Cell(currentRow, 4).Value = item.Breed ?? "N/A";
                    worksheet.Cell(currentRow, 5).Value = item.OwnerName;
                    worksheet.Cell(currentRow, 6).Value = item.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cell(currentRow, 7).Value = item.AgeDisplay;
                    worksheet.Cell(currentRow, 8).Value = item.GenderDisplay ?? "N/A";
                    worksheet.Cell(currentRow, 9).Value = item.Weight;
                    worksheet.Cell(currentRow, 10).Value = item.Color ?? "N/A";
                    worksheet.Cell(currentRow, 11).Value = item.PetId;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Mascotas_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.PetName.Contains(searchString) ||
                    p.PetCode.Contains(searchString));
            }

            var pets = await query
                .Select(p => new PetListViewModel
                {
                    PetId = p.PetId,
                    PetCode = p.PetCode,
                    PetName = p.PetName,
                    Species = p.Species.SpeciesName,
                    Breed = p.Breed != null ? p.Breed.BreedName : null,
                    OwnerName = $"{p.PetOwner.User.FirstName} {p.PetOwner.User.LastName}",
                    DateOfBirth = p.DateOfBirth,
                    GenderDisplay = p.Gender == "M" ? "Macho" : p.Gender == "F" ? "Hembra" : null,
                    Weight = p.Weight,
                    Color = p.Color
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
            var title = new Paragraph("Reporte de Mascotas")
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
            var table = new Table(new float[] { 1.5f, 2, 2, 2, 2, 2, 1.5f, 1.5f }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Código").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Especie").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Raza").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Dueño").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha Nac.").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Edad").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Género").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var pet in pets)
            {
                table.AddCell(new Cell().Add(new Paragraph(pet.PetCode).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.PetName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.Species).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.Breed ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.OwnerName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.AgeDisplay).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(pet.GenderDisplay ?? "N/A").SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Mascotas_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Where(p => p.IsActive);

            // Aplicar filtro si hay un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.PetName.Contains(searchString) ||
                    p.PetCode.Contains(searchString));
            }

            var pets = await query
                .Select(p => new PetListViewModel
                {
                    PetId = p.PetId,
                    PetCode = p.PetCode,
                    PetName = p.PetName,
                    Species = p.Species.SpeciesName,
                    Breed = p.Breed != null ? p.Breed.BreedName : null,
                    OwnerName = $"{p.PetOwner.User.FirstName} {p.PetOwner.User.LastName}",
                    DateOfBirth = p.DateOfBirth,
                    GenderDisplay = p.Gender == "M" ? "Macho" : p.Gender == "F" ? "Hembra" : null,
                    Weight = p.Weight,
                    Color = p.Color,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            // Pasar el término de búsqueda a la vista para mantenerlo en el formulario
            ViewData["CurrentFilter"] = searchString;

            return View(pets);
        }

        private static string CalculateAgeDisplay(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return "N/A";

            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;

            if (dateOfBirth.Value.Date > today.AddYears(-age))
                age--;

            if (age == 0)
            {
                var months = today.Month - dateOfBirth.Value.Month;
                if (dateOfBirth.Value.Date > today.AddMonths(-months))
                    months--;

                return $"{months} mes{(months != 1 ? "es" : "")}";
            }

            return $"{age} año{(age != 1 ? "s" : "")}";
        }

        [HttpGet]
        public async Task<IActionResult> SignUp()
        {
            // Obtener solo los usuarios que son PetOwners
            var petOwners = await _context.PetOwners
                .Include(po => po.User)
                .ToListAsync();

            ViewBag.PetOwners = petOwners.Select(po => new {
                PetOwnerId = po.PetOwnerId,
                FullName = $"{po.User.FirstName} {po.User.LastName}",
                Email = po.User.Email
            }).ToList();

            ViewBag.Species = await _context.AnimalSpecies
                .Where(s => s.IsActive)
                .ToListAsync();

            ViewBag.Breeds = await _context.Breeds
                .Where(b => b.IsActive)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(PetFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar los datos necesarios para la vista si hay error
                await ReloadViewData();
                return View(model);
            }

            try
            {
                // Generar código de mascota si no se proporcionó
                var petCode = string.IsNullOrEmpty(model.PetCode)
                    ? GeneratePetCode()
                    : model.PetCode;

                var newPet = new Pet
                {
                    PetOwnerId = model.PetOwnerId,
                    PetName = model.PetName,
                    SpeciesId = model.SpeciesId,
                    BreedId = model.BreedId,
                    Gender = model.Gender,
                    DateOfBirth = model.DateOfBirth,
                    Weight = model.Weight,
                    Color = model.Color,
                    PetCode = petCode,
                    IsActive = true
                };

                _context.Pets.Add(newPet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Mascota {model.PetName} registrada exitosamente!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar mascota");
                ModelState.AddModelError("", "Error al registrar la mascota. Intente nuevamente.");
                await ReloadViewData();
                return View(model);
            }
        }

        // GET: Pets/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(p => p.PetId == id);

            if (pet == null)
            {
                return NotFound();
            }

            await ReloadViewData(); // Cargar datos para los dropdowns

            var model = new PetFormViewModel
            {
                PetId = pet.PetId,
                PetOwnerId = pet.PetOwnerId,
                PetName = pet.PetName,
                SpeciesId = pet.SpeciesId,
                BreedId = pet.BreedId,
                Gender = pet.Gender,
                DateOfBirth = pet.DateOfBirth,
                Weight = pet.Weight,
                Color = pet.Color,
                PetCode = pet.PetCode
            };

            ViewBag.OwnerName = $"{pet.PetOwner.User.FirstName} {pet.PetOwner.User.LastName}";
            return View(model);
        }

        // POST: Pets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetFormViewModel model)
        {
            if (id != model.PetId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await ReloadViewData();
                ViewBag.OwnerName = await GetOwnerName(model.PetOwnerId);
                return View(model);
            }

            try
            {
                var pet = await _context.Pets.FindAsync(id);
                if (pet == null)
                {
                    return NotFound();
                }

                // Actualizar propiedades editables
                pet.PetName = model.PetName;
                pet.SpeciesId = model.SpeciesId;
                pet.BreedId = model.BreedId;
                pet.Gender = model.Gender;
                pet.DateOfBirth = model.DateOfBirth;
                pet.Weight = model.Weight;
                pet.Color = model.Color;
                pet.PetCode = model.PetCode!;

                _context.Update(pet);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Mascota {model.PetName} actualizada correctamente!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar mascota");
                ModelState.AddModelError("", "Ocurrió un error al guardar los cambios. Intente nuevamente.");
                await ReloadViewData();
                ViewBag.OwnerName = await GetOwnerName(model.PetOwnerId);
                return View(model);
            }
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var pet = await _context.Pets
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .FirstOrDefaultAsync(p => p.PetId == id && p.IsActive);

            if (pet == null)
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
            var title = new Paragraph($"Ficha de Mascota: {pet.PetName}")
                .SetFont(headerFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(new Cell().Add(new Paragraph("Código:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.PetCode).SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Nombre:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.PetName).SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Especie:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.Species.SpeciesName).SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Raza:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.Breed?.BreedName ?? "N/A").SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Fecha Nacimiento:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.DateOfBirth?.ToString("dd/MM/yyyy") ?? "N/A").SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Edad:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(CalculateAgeDisplay(pet.DateOfBirth)).SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Género:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(
                pet.Gender == "M" ? "Macho" : pet.Gender == "F" ? "Hembra" : "No especificado")
                .SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Peso:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.Weight?.ToString("0.0") + " kg" ?? "N/A").SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Color:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(pet.Color ?? "N/A").SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Dueño:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            infoTable.AddCell(new Cell().Add(new Paragraph(
                $"{pet.PetOwner.User.FirstName} {pet.PetOwner.User.LastName}")
                .SetFont(normalFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));

            infoTable.AddCell(new Cell().Add(new Paragraph("Estado:").SetFont(boldFont)).SetPadding(5).SetTextAlignment(TextAlignment.LEFT));
            var statusCell = new Cell().Add(new Paragraph(pet.IsActive ? "Activo" : "Inactivo").SetFont(normalFont))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT);
            statusCell.SetBackgroundColor(pet.IsActive ?
                new DeviceRgb(40, 167, 69) : new DeviceRgb(108, 117, 125));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            document.Add(infoTable);

            // Información adicional
            var notesTitle = new Paragraph("Información Adicional")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetMarginBottom(10);
            document.Add(notesTitle);

            var notesContent = new Paragraph("Para ver el historial médico completo, vacunas y otros registros, consulte el sistema.")
                .SetFont(normalFont)
                .SetMarginBottom(20);
            document.Add(notesContent);

            // Pie de página
            var footer = new Paragraph($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {pet.PetId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf", $"Mascota_{pet.PetName}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Include(p => p.Species)
                .Include(p => p.Breed)
                .FirstOrDefaultAsync(m => m.PetId == id && m.IsActive);

            if (pet == null)
            {
                return NotFound();
            }

            var viewModel = new PetListViewModel
            {
                PetId = pet.PetId,
                PetCode = pet.PetCode,
                PetName = pet.PetName,
                Species = pet.Species.SpeciesName,
                Breed = pet.Breed?.BreedName,
                OwnerName = $"{pet.PetOwner.User.FirstName} {pet.PetOwner.User.LastName}",
                DateOfBirth = pet.DateOfBirth,
                GenderDisplay = pet.Gender == "M" ? "Macho" : pet.Gender == "F" ? "Hembra" : "No especificado",
                Weight = pet.Weight,
                Color = pet.Color,
                IsActive = pet.IsActive
            };

            return View(viewModel);
        }

        private async Task<string> GetOwnerName(int petOwnerId)
        {
            var owner = await _context.PetOwners
                .Include(po => po.User)
                .FirstOrDefaultAsync(po => po.PetOwnerId == petOwnerId);

            return owner != null ? $"{owner.User.FirstName} {owner.User.LastName}" : "Dueño desconocido";
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(id);
                if (pet == null)
                {
                    return NotFound();
                }

                pet.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Mascota {pet.PetName} eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar mascota");
                TempData["ErrorMessage"] = "Error al eliminar la mascota. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task ReloadViewData()
        {
            ViewBag.PetOwners = await _context.PetOwners
                .Include(po => po.User)
                .Select(po => new {
                    PetOwnerId = po.PetOwnerId,
                    FullName = $"{po.User.FirstName} {po.User.LastName}",
                    Email = po.User.Email
                })
                .ToListAsync();

            ViewBag.Species = await _context.AnimalSpecies
                .Where(s => s.IsActive)
                .ToListAsync();

            ViewBag.Breeds = await _context.Breeds
                .Where(b => b.IsActive)
                .ToListAsync();
        }

        private string GeneratePetCode()
        {
            // Generar un código único para la mascota (ejemplo simple)
            return $"PET-{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
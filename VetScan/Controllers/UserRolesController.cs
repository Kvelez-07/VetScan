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
    public class UserRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(ApplicationDbContext context, ILogger<UserRolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            var query = _context.UserRoles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(r =>
                    r.RoleName.Contains(searchString) ||
                    (r.Description != null && r.Description.Contains(searchString)));
            }

            var roles = await query
                .OrderBy(r => r.RoleName)
                .Select(r => new UserRoleListViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.Users.Count
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("RolesUsuario");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Nombre";
                worksheet.Cell(currentRow, 2).Value = "Descripción";
                worksheet.Cell(currentRow, 3).Value = "Usuarios";
                worksheet.Cell(currentRow, 4).Value = "Estado";
                worksheet.Cell(currentRow, 5).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 5);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in roles)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.RoleName;
                    worksheet.Cell(currentRow, 2).Value = item.Description ?? "N/A";
                    worksheet.Cell(currentRow, 3).Value = item.UserCount;
                    worksheet.Cell(currentRow, 4).Value = item.IsActive ? "Activo" : "Inactivo";
                    worksheet.Cell(currentRow, 5).Value = item.RoleId;

                    // Color para estado
                    var statusCell = worksheet.Cell(currentRow, 4);
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
                        $"RolesUsuario_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            // Obtener los datos (igual que en el Index)
            var query = _context.UserRoles.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(r =>
                    r.RoleName.Contains(searchString) ||
                    (r.Description != null && r.Description.Contains(searchString)));
            }

            var roles = await query
                .OrderBy(r => r.RoleName)
                .Select(r => new UserRoleListViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.Users.Count
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
            var title = new Paragraph("Reporte de Roles de Usuario")
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
            var table = new Table(new float[] { 2, 3, 1, 1, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Usuarios").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in roles)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.RoleName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Description ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.UserCount.ToString()).SetFont(normalFont)));

                var statusCell = new Cell().Add(new Paragraph(item.IsActive ? "Activo" : "Inactivo").SetFont(normalFont));
                statusCell.SetBackgroundColor(item.IsActive ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
                statusCell.SetFontColor(new DeviceRgb(255, 255, 255));
                table.AddCell(statusCell);

                table.AddCell(new Cell().Add(new Paragraph(item.RoleId.ToString()).SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"RolesUsuario_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: UserRoles
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.UserRoles
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(r =>
                    r.RoleName.Contains(searchString) ||
                    (r.Description != null && r.Description.Contains(searchString)));
            }

            var roles = await query
                .OrderBy(r => r.RoleName)
                .Select(r => new UserRoleListViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.Users.Count
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(roles);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null) return NotFound();

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
            var title = new Paragraph("DETALLES DE ROL DE USUARIO")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Nombre del Rol:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(role.RoleName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Estado:", boldFont, TextAlignment.LEFT));
            var statusCell = CreateCell(role.IsActive ? "Activo" : "Inactivo", normalFont, TextAlignment.LEFT);
            statusCell.SetBackgroundColor(role.IsActive ? new DeviceRgb(40, 167, 69) : new DeviceRgb(220, 53, 69));
            statusCell.SetFontColor(DeviceRgb.WHITE);
            infoTable.AddCell(statusCell);

            infoTable.AddCell(CreateCell("Usuarios asignados:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(role.Users.Count.ToString(), normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Descripción
            var descriptionTitle = new Paragraph("DESCRIPCIÓN DEL ROL")
                .SetFont(headerFont)
                .SetFontSize(14)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(descriptionTitle);

            var descriptionContent = new Paragraph(string.IsNullOrEmpty(role.Description) ?
                "No hay descripción disponible para este rol." : role.Description)
                .SetFont(normalFont)
                .SetMarginBottom(20)
                .SetPaddingLeft(20)
                .SetPaddingRight(20);
            document.Add(descriptionContent);

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {role.RoleId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Rol_{role.RoleName}_{DateTime.Now:yyyyMMdd}.pdf");
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
            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null) return NotFound();

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/UserRoles/Details/{id}?exporting=true";

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
                Height = 900,
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
                $"Rol_{role.RoleName}_{DateTime.Now:yyyyMMdd}.{fileExtension}");
        }

        // GET: UserRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (role == null) return NotFound();

            var viewModel = new UserRoleListViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                UserCount = role.Users.Count
            };

            return View(viewModel);
        }

        // GET: UserRoles/Create
        public IActionResult Create() => View();

        // POST: UserRoles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRoleFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var role = new UserRole
                    {
                        RoleName = model.RoleName,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    _context.Add(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Rol creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear rol");
                    ModelState.AddModelError("", "No se pudo crear el rol. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: UserRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.UserRoles.FindAsync(id);
            if (role == null) return NotFound();

            var model = new UserRoleFormViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive
            };

            return View(model);
        }

        // POST: UserRoles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserRoleFormViewModel model)
        {
            if (id != model.RoleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var role = await _context.UserRoles.FindAsync(id);
                    if (role == null) return NotFound();

                    role.RoleName = model.RoleName;
                    role.Description = model.Description;
                    role.IsActive = model.IsActive;

                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Rol actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar rol");
                    ModelState.AddModelError("", "No se pudo actualizar el rol. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: UserRoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (role == null) return NotFound();

            if (role.Users.Any())
            {
                TempData["ErrorMessage"] = "No se puede desactivar el rol porque tiene usuarios asociados";
                return RedirectToAction(nameof(Index));
            }

            return View(role);
        }

        // POST: UserRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleId == id);

            if (role == null)
            {
                TempData["ErrorMessage"] = "Rol no encontrado";
                return RedirectToAction(nameof(Index));
            }

            if (role.Users.Any())
            {
                TempData["ErrorMessage"] = "No se puede desactivar el rol porque tiene usuarios asociados";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                role.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rol desactivado exitosamente";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al desactivar rol");
                TempData["ErrorMessage"] = "No se pudo desactivar el rol. Intente nuevamente.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
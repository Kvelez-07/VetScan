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
    public class AppUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppUsersController> _logger;

        public AppUsersController(ApplicationDbContext context, ILogger<AppUsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            var query = _context.AppUsers
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.FirstName.Contains(searchString) ||
                    u.LastName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchString)));
            }

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new AppUsersListViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.Role.RoleName
                })
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Usuarios");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "Usuario";
                worksheet.Cell(currentRow, 2).Value = "Nombre";
                worksheet.Cell(currentRow, 3).Value = "Apellido";
                worksheet.Cell(currentRow, 4).Value = "Correo";
                worksheet.Cell(currentRow, 5).Value = "Teléfono";
                worksheet.Cell(currentRow, 6).Value = "Rol";
                worksheet.Cell(currentRow, 7).Value = "ID";

                // Formato de encabezados
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 7);
                headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;

                // Datos
                foreach (var item in users)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.Username;
                    worksheet.Cell(currentRow, 2).Value = item.FirstName;
                    worksheet.Cell(currentRow, 3).Value = item.LastName;
                    worksheet.Cell(currentRow, 4).Value = item.Email;
                    worksheet.Cell(currentRow, 5).Value = item.PhoneNumber ?? "N/A";
                    worksheet.Cell(currentRow, 6).Value = item.RoleName;
                    worksheet.Cell(currentRow, 7).Value = item.UserId;
                }

                // Ajustar ancho de columnas
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf(string searchString)
        {
            var query = _context.AppUsers
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.FirstName.Contains(searchString) ||
                    u.LastName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchString)));
            }

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new AppUsersListViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.Role.RoleName
                })
                .ToListAsync();

            // Configuración del PDF
            var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4.Rotate());

            // Fuentes
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Título
            var title = new Paragraph("Reporte de Usuarios")
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
            var table = new Table(new float[] { 2, 2, 2, 2, 2, 1 }, true)
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Encabezados de tabla
            table.AddHeaderCell(new Cell().Add(new Paragraph("Usuario").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Correo").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Rol").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Teléfono").SetFont(headerFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetFont(headerFont)));

            // Datos de la tabla
            foreach (var item in users)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.Username).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph($"{item.FirstName} {item.LastName}").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.Email).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.RoleName).SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.PhoneNumber ?? "N/A").SetFont(normalFont)));
                table.AddCell(new Cell().Add(new Paragraph(item.UserId.ToString()).SetFont(normalFont)));
            }

            document.Add(table);
            document.Close();

            return File(memoryStream.ToArray(), "application/pdf", $"Usuarios_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // GET: AppUsers
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.AppUsers
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.FirstName.Contains(searchString) ||
                    u.LastName.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchString)));
            }

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new AppUsersListViewModel
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.Role.RoleName
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(users);
        }

        public async Task<IActionResult> ExportDetailsToPdf(int id)
        {
            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
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
            var title = new Paragraph("FICHA DE USUARIO")
                .SetFont(headerFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(title);

            // Información básica
            var infoTable = new Table(new float[] { 3, 7 })
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(20);

            infoTable.AddCell(CreateCell("Usuario:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(user.Username, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Nombre completo:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell($"{user.FirstName} {user.LastName}", normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Correo electrónico:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(user.Email, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Rol:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(user.Role.RoleName, normalFont, TextAlignment.LEFT));

            infoTable.AddCell(CreateCell("Teléfono:", boldFont, TextAlignment.LEFT));
            infoTable.AddCell(CreateCell(user.PhoneNumber ?? "N/A", normalFont, TextAlignment.LEFT));

            document.Add(infoTable);

            // Pie de página
            var footer = new Paragraph($"Documento generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm")} | ID: {user.UserId}")
                .SetFont(normalFont)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(DeviceRgb.BLACK);
            document.Add(footer);

            document.Close();
            return File(memoryStream.ToArray(), "application/pdf",
                $"Usuario_{user.Username}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private Cell CreateCell(string text, PdfFont font, TextAlignment alignment)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetPadding(5)
                .SetTextAlignment(alignment);
        }

        // GET: AppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new AppUsersListViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role.RoleName
            };

            return View(viewModel);
        }

        // GET: AppUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new AppUsersFormViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                AvailableRoles = await _context.UserRoles.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppUsersFormViewModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.AppUsers.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Actualizar propiedades editables
                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.RoleId = model.RoleId;

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Usuario actualizado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar usuario");
                    ModelState.AddModelError("", "Error al actualizar. Intente nuevamente.");
                }
            }

            // Recargar roles si hay error
            model.AvailableRoles = await _context.UserRoles.ToListAsync();
            return View(model);
        }

        // POST: AppUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Usuario eliminado correctamente";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                TempData["ErrorMessage"] = "Error al eliminar el usuario";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /AppUsers/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: /AppUsers/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            return RedirectToAction("Index", "Home");
        }

        // GET: /AppUsers/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            ViewBag.Roles = _context.UserRoles.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            ViewBag.Roles = _context.UserRoles.ToList();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (await _context.AppUsers.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "El nombre de usuario ya está en uso");
                    return View(model);
                }

                if (await _context.AppUsers.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está registrado");
                    return View(model);
                }

                var newUser = new AppUser
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    RoleId = model.RoleId
                };

                _context.AppUsers.Add(newUser);
                await _context.SaveChangesAsync();

                if (model.RoleId == 1) // Rol Admin
                    return RedirectToAction("CompleteAdminRegistration", new { userId = newUser.UserId });
                if (model.RoleId == 2) // Rol Veterinarian
                    return RedirectToAction("CompleteVeterinarianRegistration", new { userId = newUser.UserId });
                if (model.RoleId == 3) // Rol PetOwner
                    return RedirectToAction("CompletePetOwnerRegistration", new { userId = newUser.UserId });

                TempData["SuccessMessage"] = "¡Registro exitoso!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                ModelState.AddModelError("", "Error interno. Intente nuevamente.");
                return View(model);
            }
        }

        // GET: /AppUsers/CompleteVeterinarianRegistration
        [HttpGet]
        public IActionResult CompleteVeterinarianRegistration(int userId)
        {
            var user = _context.AppUsers.Find(userId);
            if (user == null) return NotFound();

            ViewBag.UserId = userId;
            ViewBag.Specialties = _context.Specialties.Where(s => s.IsActive).ToList();

            var model = new VeterinarianFormViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // POST: /AppUsers/CompleteVeterinarianRegistration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteVeterinarianRegistration(
            [FromForm] int userId,
            VeterinarianFormViewModel model)
        {
            ViewBag.Specialties = _context.Specialties.Where(s => s.IsActive).ToList();

            if (!ModelState.IsValid)
            {
                ViewBag.UserId = userId;
                return View(model);
            }

            try
            {
                var veterinarian = new Veterinarian
                {
                    UserId = userId,
                    YearsOfExperience = model.YearsOfExperience,
                    Education = model.Education,
                    SpecialtyId = model.SpecialtyId
                };

                _context.Veterinarians.Add(veterinarian);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Registro de veterinario completado con éxito!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar registro de veterinario");
                ModelState.AddModelError("", "Error al guardar los datos. Intente nuevamente.");
                ViewBag.UserId = userId;
                return View(model);
            }
        }

        // GET: /AppUsers/CompletePetOwnerRegistration
        [HttpGet]
        public IActionResult CompletePetOwnerRegistration(int userId)
        {
            var user = _context.AppUsers.Find(userId);
            if (user == null) return NotFound();

            ViewBag.UserId = userId;

            var model = new PetOwnerFormViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        // POST: /AppUsers/CompletePetOwnerRegistration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePetOwnerRegistration(
            [FromForm] int userId,
            PetOwnerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = userId;
                return View(model);
            }

            try
            {
                var petOwner = new PetOwner
                {
                    UserId = userId,
                    Address = model.Address,
                    City = model.City,
                    State = model.State,
                    PostalCode = model.PostalCode,
                    Country = model.Country,
                    EmergencyContactName = model.EmergencyContactName,
                    EmergencyContactPhone = model.EmergencyContactPhone,
                    PreferredContactMethod = model.PreferredContactMethod
                };

                _context.PetOwners.Add(petOwner);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Registro de dueño de mascota completado con éxito!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar registro de dueño de mascota");
                ModelState.AddModelError("", "Error al guardar los datos. Intente nuevamente.");
                ViewBag.UserId = userId;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult CompleteAdminRegistration(int userId)
        {
            var user = _context.AppUsers.Find(userId);
            if (user == null) return NotFound();

            ViewBag.UserId = userId; // Importante para el formulario

            var model = new AdminFormViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAdminRegistration(
            [FromForm] int userId,
            AdminFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = userId; // Mantener el userId en caso de error
                return View(model);
            }

            try
            {
                var adminStaff = new AdminStaff
                {
                    UserId = userId,
                    Position = model.Position,
                    Department = model.Department,
                    HireDate = DateTime.UtcNow,
                    Salary = model.Salary
                };

                _context.AdminStaff.Add(adminStaff);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Registro de administrador completado con éxito!";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar registro de administrador");
                ModelState.AddModelError("", "Error al guardar los datos. Intente nuevamente.");
                ViewBag.UserId = userId;
                return View(model);
            }
        }

        // GET: /AppUsers/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword() => View();

        // POST: /AppUsers/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar usuario en la base de datos
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.CurrentPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contraseña actual incorrectos");
                return View(model);
            }

            // Actualizar contraseña directamente (sin hash por ahora)
            user.Password = model.NewPassword;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Contraseña actualizada correctamente";
            return RedirectToAction("Login");
        }
    }
}
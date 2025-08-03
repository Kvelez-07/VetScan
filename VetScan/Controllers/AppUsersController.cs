using AspNetCore.ReCaptcha;
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
using System.ComponentModel.DataAnnotations;
using VetScan.Data;
using VetScan.Data.Services;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class AppUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppUsersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AppUsersController(ApplicationDbContext context, ILogger<AppUsersController> logger, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
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

            if (user == null) return NotFound();

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

        // Método para exportar detalles a imagen
        public async Task<IActionResult> ExportDetailsToImage(int id, string format = "png")
        {
            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            // Configurar la URL para la vista
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/AppUsers/Details/{id}?exporting=true";

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
                $"Usuario_{user.Username}_{DateTime.Now:yyyyMMdd}.{fileExtension}");
        }

        // GET: AppUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

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
            if (id == null) return NotFound();

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

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
            if (id != model.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.AppUsers.FindAsync(id);
                    if (user == null) return NotFound();

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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();

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

        // POST: AppUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (string.IsNullOrEmpty(model.RecaptchaToken))
            {
                ModelState.AddModelError("", "Por favor complete la verificación reCAPTCHA.");
                return View(model);
            }

            // Determine if the input is an email address
            var isEmail = new EmailAddressAttribute().IsValid(model.UsernameOrEmail);

            // Find user by username or email
            var user = await _context.AppUsers
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    (isEmail && u.Email == model.UsernameOrEmail) ||
                    (!isEmail && u.Username == model.UsernameOrEmail));

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario/Email o contraseña incorrectos");
                return View(model);
            }

            // Verify password (you should use password hashing in production)
            if (user.Password != model.Password)
            {
                ModelState.AddModelError("", "Usuario/Email o contraseña incorrectos");
                return View(model);
            }

            // Verify Role exists
            if (user.Role == null)
            {
                ModelState.AddModelError("", "El usuario no tiene un rol asignado");
                return View(model);
            }

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email ?? string.Empty);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            return RedirectToAction("Index", "Home");
        }

        // GET: /AppUsers/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
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
        [ValidateReCaptcha]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            ViewBag.Roles = _context.UserRoles.ToList();

            if (!ModelState.IsValid)
                return View(model);

            // Verify reCAPTCHA
            if (string.IsNullOrEmpty(model.RecaptchaToken))
            {
                ModelState.AddModelError("", "Por favor complete la verificación reCAPTCHA.");
                return View(model);
            }

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
                return RedirectToAction(nameof(Login));
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
                return RedirectToAction(nameof(Login));
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
                return RedirectToAction(nameof(Login));
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
                return RedirectToAction(nameof(Login));
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
        [ValidateReCaptcha]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verify reCAPTCHA
            if (string.IsNullOrEmpty(model.RecaptchaToken))
            {
                ModelState.AddModelError("", "Por favor complete la verificación reCAPTCHA.");
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
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult RequestPasswordReset() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Siempre devuelve éxito para no revelar qué emails existen en el sistema
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return RedirectToAction(nameof(ResetEmailSent));

            // Generar token seguro
            var token = Guid.NewGuid().ToString();
            var expiration = DateTime.UtcNow.AddHours(1); // 1 hora de expiración

            // Eliminar tokens antiguos para este email
            var oldTokens = await _context.PasswordResetTokens
                .Where(t => t.Email == model.Email)
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(oldTokens);

            // Guardar nuevo token
            var resetToken = new PasswordResetToken
            {
                Email = model.Email,
                Token = token,
                ExpirationDate = expiration,
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Crear enlace de reset
            var resetUrl = Url.Action(
                "ResetPassword",
                "AppUsers",
                new { email = model.Email, token = token },
                protocol: Request.Scheme);

            // Plantilla de email más profesional
            var emailSubject = "Instrucciones para restablecer tu contraseña en VetScan";
            var emailBody = $@"
    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
        <h2 style='color: #2c3e50;'>Restablecimiento de contraseña</h2>
        <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en VetScan.</p>
        <p>Por favor, haz clic en el siguiente enlace para continuar:</p>
        <p style='margin: 20px 0;'>
            <a href='{resetUrl}' style='background-color: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                Restablecer contraseña
            </a>
        </p>
        <p>Si no puedes hacer clic en el botón, copia y pega esta URL en tu navegador:</p>
        <p><code>{resetUrl}</code></p>
        <p>Este enlace expirará en 1 hora. Si no solicitaste este cambio, puedes ignorar este mensaje.</p>
        <p style='margin-top: 30px; color: #7f8c8d; font-size: 0.9em;'>
            Atentamente,<br>
            El equipo de VetScan
        </p>
    </div>";

            try
            {
                await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
                return RedirectToAction(nameof(ResetEmailSent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", model.Email);
                ModelState.AddModelError("", "Ocurrió un error al enviar el correo. Por favor intenta nuevamente.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetEmailSent() => View();

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return RedirectToAction(nameof(RequestPasswordReset));

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate token
            var validToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t =>
                    t.Email == model.Email &&
                    t.Token == model.Token &&
                    !t.IsUsed &&
                    t.ExpirationDate > DateTime.UtcNow);

            if (validToken == null)
            {
                ModelState.AddModelError("", "Invalid or expired token.");
                return View(model);
            }

            // Update password
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                user.Password = model.NewPassword;
                validToken.IsUsed = true;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Password updated successfully. Please login with your new password.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult PasswordResetEmail() => View();
    }
}
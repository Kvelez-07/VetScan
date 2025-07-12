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
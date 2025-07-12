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
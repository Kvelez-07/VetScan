using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace VetScan.Controllers
{
    public class VaccinationHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccinationHistoriesController> _logger;

        public VaccinationHistoriesController(ApplicationDbContext context, ILogger<VaccinationHistoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(v =>
                    v.Pet.PetName.Contains(searchString) ||
                    v.Vaccine.VaccineName.Contains(searchString) ||
                    (v.Veterinarian.User.FirstName + " " + v.Veterinarian.User.LastName).Contains(searchString) ||
                    v.BatchNumber!.Contains(searchString));
            }

            var vaccinations = await query
                .OrderByDescending(v => v.VaccinationDate)
                .Select(v => new VaccinationHistoryListViewModel
                {
                    VaccinationId = v.VaccinationId,
                    PetName = v.Pet.PetName,
                    VaccineName = v.Vaccine.VaccineName,
                    VeterinarianName = $"{v.Veterinarian.User.FirstName} {v.Veterinarian.User.LastName}",
                    VaccinationDate = v.VaccinationDate,
                    BatchNumber = v.BatchNumber!,
                    NextDueDate = v.NextDueDate
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(vaccinations);
        }

        // GET: VaccinationHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccination = await _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .FirstOrDefaultAsync(m => m.VaccinationId == id);

            if (vaccination == null)
            {
                return NotFound();
            }

            var viewModel = new VaccinationHistoryListViewModel
            {
                VaccinationId = vaccination.VaccinationId,
                PetName = vaccination.Pet.PetName,
                VaccineName = vaccination.Vaccine.VaccineName,
                VeterinarianName = $"{vaccination.Veterinarian.User.FirstName} {vaccination.Veterinarian.User.LastName}",
                VaccinationDate = vaccination.VaccinationDate,
                BatchNumber = vaccination.BatchNumber!,
                NextDueDate = vaccination.NextDueDate
            };

            ViewBag.ExpirationDate = vaccination.ExpirationDate?.ToString("dd/MM/yyyy");
            ViewBag.Reactions = vaccination.Reactions;
            ViewBag.VaccineType = vaccination.Vaccine.VaccineType;
            ViewBag.VaccineManufacturer = vaccination.Vaccine.Manufacturer;

            return View(viewModel);
        }

        // GET: VaccinationHistories/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new VaccinationHistoryFormViewModel
            {
                VaccinationDate = DateTime.Today
            });
        }

        // POST: VaccinationHistories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VaccinationHistoryFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vaccination = new VaccinationHistory
                    {
                        PetId = model.PetId,
                        VaccineId = model.VaccineId,
                        VeterinarianId = model.VeterinarianId,
                        VaccinationDate = model.VaccinationDate,
                        BatchNumber = model.BatchNumber,
                        ExpirationDate = model.ExpirationDate,
                        NextDueDate = model.NextDueDate,
                        Reactions = model.Reactions
                    };

                    _context.Add(vaccination);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vacunación registrada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al registrar vacunación");
                    ModelState.AddModelError("", "No se pudo registrar la vacunación. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: VaccinationHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccination = await _context.VaccinationHistories.FindAsync(id);
            if (vaccination == null)
            {
                return NotFound();
            }

            var model = new VaccinationHistoryFormViewModel
            {
                VaccinationId = vaccination.VaccinationId,
                PetId = vaccination.PetId,
                VaccineId = vaccination.VaccineId,
                VeterinarianId = vaccination.VeterinarianId,
                VaccinationDate = vaccination.VaccinationDate,
                BatchNumber = vaccination.BatchNumber!,
                ExpirationDate = vaccination.ExpirationDate,
                NextDueDate = vaccination.NextDueDate,
                Reactions = vaccination.Reactions!
            };

            await LoadViewData();
            ViewBag.PetName = (await _context.Pets.FindAsync(vaccination.PetId))?.PetName;
            ViewBag.VaccineName = (await _context.Vaccines.FindAsync(vaccination.VaccineId))?.VaccineName;
            ViewBag.VeterinarianName = (await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VeterinarianId == vaccination.VeterinarianId))?.User.FirstName;

            return View(model);
        }

        // POST: VaccinationHistories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VaccinationHistoryFormViewModel model)
        {
            if (id != model.VaccinationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var vaccination = await _context.VaccinationHistories.FindAsync(id);
                    if (vaccination == null)
                    {
                        return NotFound();
                    }

                    vaccination.VaccinationDate = model.VaccinationDate;
                    vaccination.BatchNumber = model.BatchNumber;
                    vaccination.ExpirationDate = model.ExpirationDate;
                    vaccination.NextDueDate = model.NextDueDate;
                    vaccination.Reactions = model.Reactions;

                    _context.Update(vaccination);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Vacunación actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar vacunación");
                    ModelState.AddModelError("", "No se pudo actualizar la vacunación. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: VaccinationHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vaccination = await _context.VaccinationHistories
                .Include(v => v.Pet)
                .Include(v => v.Vaccine)
                .Include(v => v.Veterinarian)
                    .ThenInclude(vet => vet.User)
                .FirstOrDefaultAsync(m => m.VaccinationId == id);

            if (vaccination == null)
            {
                return NotFound();
            }

            return View(vaccination);
        }

        // POST: VaccinationHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vaccination = await _context.VaccinationHistories.FindAsync(id);
            if (vaccination != null)
            {
                _context.VaccinationHistories.Remove(vaccination);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Vacunación eliminada exitosamente";
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

            // Cargar vacunas activas
            ViewBag.Vaccines = await _context.Vaccines
                .Where(v => v.IsActive)
                .OrderBy(v => v.VaccineName)
                .Select(v => new
                {
                    VaccineId = v.VaccineId,
                    DisplayText = v.VaccineName
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
        }
    }
}
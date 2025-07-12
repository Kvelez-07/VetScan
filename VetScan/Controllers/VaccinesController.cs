// Controllers/VaccinesController.cs
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
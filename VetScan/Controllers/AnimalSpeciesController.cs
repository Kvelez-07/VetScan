using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: AnimalSpecies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var species = await _context.AnimalSpecies
                .Include(s => s.Breeds)
                .Include(s => s.Pets)
                .FirstOrDefaultAsync(m => m.SpeciesId == id);

            if (species == null)
            {
                return NotFound();
            }

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
            if (species == null)
            {
                return NotFound();
            }

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
            if (id != model.SpeciesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var species = await _context.AnimalSpecies.FindAsync(id);
                    if (species == null)
                    {
                        return NotFound();
                    }

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
            if (id == null)
            {
                return NotFound();
            }

            var species = await _context.AnimalSpecies
                .Include(s => s.Breeds)
                .Include(s => s.Pets)
                .FirstOrDefaultAsync(m => m.SpeciesId == id);

            if (species == null)
            {
                return NotFound();
            }

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
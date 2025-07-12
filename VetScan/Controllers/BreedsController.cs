// BreedsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class BreedsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BreedsController> _logger;

        public BreedsController(ApplicationDbContext context, ILogger<BreedsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Breeds
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Breeds
                .Include(b => b.Species)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b =>
                    b.BreedName.Contains(searchString) ||
                    (b.Description != null && b.Description.Contains(searchString)) ||
                    b.Species.SpeciesName.Contains(searchString));
            }

            var breeds = await query
                .OrderBy(b => b.Species.SpeciesName)
                .ThenBy(b => b.BreedName)
                .Select(b => new BreedListViewModel
                {
                    BreedId = b.BreedId,
                    BreedName = b.BreedName,
                    SpeciesName = b.Species.SpeciesName,
                    Description = b.Description,
                    IsActive = b.IsActive
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(breeds);
        }

        // GET: Breeds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var breed = await _context.Breeds
                .Include(b => b.Species)
                .FirstOrDefaultAsync(m => m.BreedId == id);

            if (breed == null)
            {
                return NotFound();
            }

            var viewModel = new BreedListViewModel
            {
                BreedId = breed.BreedId,
                BreedName = breed.BreedName,
                SpeciesName = breed.Species.SpeciesName,
                Description = breed.Description,
                IsActive = breed.IsActive
            };

            return View(viewModel);
        }

        // GET: Breeds/Create
        public async Task<IActionResult> Create()
        {
            await LoadSpeciesViewData();
            return View();
        }

        // POST: Breeds/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BreedFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var breed = new Breed
                    {
                        SpeciesId = model.SpeciesId,
                        BreedName = model.BreedName,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    _context.Add(breed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Raza creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear raza");
                    ModelState.AddModelError("", "No se pudo crear la raza. Intente nuevamente.");
                }
            }

            await LoadSpeciesViewData();
            return View(model);
        }

        // GET: Breeds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var breed = await _context.Breeds.FindAsync(id);
            if (breed == null)
            {
                return NotFound();
            }

            var model = new BreedFormViewModel
            {
                BreedId = breed.BreedId,
                SpeciesId = breed.SpeciesId,
                BreedName = breed.BreedName,
                Description = breed.Description,
                IsActive = breed.IsActive
            };

            await LoadSpeciesViewData();
            ViewBag.SpeciesName = (await _context.AnimalSpecies.FindAsync(breed.SpeciesId))?.SpeciesName;
            return View(model);
        }

        // POST: Breeds/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BreedFormViewModel model)
        {
            if (id != model.BreedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var breed = await _context.Breeds.FindAsync(id);
                    if (breed == null)
                    {
                        return NotFound();
                    }

                    breed.SpeciesId = model.SpeciesId;
                    breed.BreedName = model.BreedName;
                    breed.Description = model.Description;
                    breed.IsActive = model.IsActive;

                    _context.Update(breed);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Raza actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar raza");
                    ModelState.AddModelError("", "No se pudo actualizar la raza. Intente nuevamente.");
                }
            }

            await LoadSpeciesViewData();
            return View(model);
        }

        // GET: Breeds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var breed = await _context.Breeds
                .Include(b => b.Species)
                .FirstOrDefaultAsync(m => m.BreedId == id);

            if (breed == null)
            {
                return NotFound();
            }

            return View(breed);
        }

        // POST: Breeds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var breed = await _context.Breeds.FindAsync(id);
            if (breed != null)
            {
                breed.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Raza desactivada exitosamente";
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
        }
    }
}
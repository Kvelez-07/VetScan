// SpecialtiesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class SpecialtiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpecialtiesController> _logger;

        public SpecialtiesController(ApplicationDbContext context, ILogger<SpecialtiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Specialties
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Specialties
                .Include(s => s.Veterinarians)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.SpecialtyName.Contains(searchString) ||
                    (s.Description != null && s.Description.Contains(searchString)));
            }

            var specialties = await query
                .OrderBy(s => s.SpecialtyName)
                .Select(s => new SpecialtyListViewModel
                {
                    SpecialtyId = s.SpecialtyId,
                    SpecialtyName = s.SpecialtyName,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    VeterinarianCount = s.Veterinarians.Count
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(specialties);
        }

        // GET: Specialties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties
                .Include(s => s.Veterinarians)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.SpecialtyId == id);

            if (specialty == null)
            {
                return NotFound();
            }

            var viewModel = new SpecialtyListViewModel
            {
                SpecialtyId = specialty.SpecialtyId,
                SpecialtyName = specialty.SpecialtyName,
                Description = specialty.Description,
                IsActive = specialty.IsActive,
                VeterinarianCount = specialty.Veterinarians.Count
            };

            ViewBag.Veterinarians = specialty.Veterinarians
                .Select(v => $"{v.User.FirstName} {v.User.LastName}")
                .ToList();

            return View(viewModel);
        }

        // GET: Specialties/Create
        public IActionResult Create() => View();

        // POST: Specialties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpecialtyFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var specialty = new Specialty
                    {
                        SpecialtyName = model.SpecialtyName,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    _context.Add(specialty);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Especialidad creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear especialidad");
                    ModelState.AddModelError("", "No se pudo crear la especialidad. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: Specialties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null)
            {
                return NotFound();
            }

            var model = new SpecialtyFormViewModel
            {
                SpecialtyId = specialty.SpecialtyId,
                SpecialtyName = specialty.SpecialtyName,
                Description = specialty.Description,
                IsActive = specialty.IsActive
            };

            return View(model);
        }

        // POST: Specialties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SpecialtyFormViewModel model)
        {
            if (id != model.SpecialtyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var specialty = await _context.Specialties.FindAsync(id);
                    if (specialty == null)
                    {
                        return NotFound();
                    }

                    specialty.SpecialtyName = model.SpecialtyName;
                    specialty.Description = model.Description;
                    specialty.IsActive = model.IsActive;

                    _context.Update(specialty);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Especialidad actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar especialidad");
                    ModelState.AddModelError("", "No se pudo actualizar la especialidad. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: Specialties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialty = await _context.Specialties
                .Include(s => s.Veterinarians)
                .FirstOrDefaultAsync(m => m.SpecialtyId == id);

            if (specialty == null)
            {
                return NotFound();
            }

            if (specialty.Veterinarians.Any())
            {
                TempData["ErrorMessage"] = "No se puede desactivar la especialidad porque tiene veterinarios asociados";
                return RedirectToAction(nameof(Index));
            }

            return View(specialty);
        }

        // POST: Specialties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialty = await _context.Specialties
                .Include(s => s.Veterinarians)
                .FirstOrDefaultAsync(s => s.SpecialtyId == id);

            if (specialty == null)
            {
                return NotFound();
            }

            if (specialty.Veterinarians.Any())
            {
                TempData["ErrorMessage"] = "No se puede desactivar la especialidad porque tiene veterinarios asociados";
                return RedirectToAction(nameof(Index));
            }

            specialty.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Especialidad desactivada exitosamente";
            return RedirectToAction(nameof(Index));
        }
    }
}
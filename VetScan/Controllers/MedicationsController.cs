// Controllers/MedicationsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class MedicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicationsController> _logger;

        public MedicationsController(ApplicationDbContext context, ILogger<MedicationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Medications
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.Medications
                .Where(m => m.IsActive);

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(m =>
                    m.MedicationName.Contains(searchString) ||
                    (m.Manufacturer != null && m.Manufacturer.Contains(searchString)));
            }

            var medications = await query
                .OrderBy(m => m.MedicationName)
                .Select(m => new MedicationListViewModel
                {
                    MedicationId = m.MedicationId,
                    MedicationName = m.MedicationName,
                    GenericName = m.GenericName,
                    Manufacturer = m.Manufacturer,
                    Concentration = m.Concentration,
                    Category = m.Category
                })
                .ToListAsync();

            // Pasar el término de búsqueda a la vista
            ViewData["CurrentFilter"] = searchString;

            return View(medications);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications
                .FirstOrDefaultAsync(m => m.MedicationId == id && m.IsActive);

            if (medication == null)
            {
                return NotFound();
            }

            var viewModel = new MedicationListViewModel
            {
                MedicationId = medication.MedicationId,
                MedicationName = medication.MedicationName,
                GenericName = medication.GenericName,
                Manufacturer = medication.Manufacturer,
                Concentration = medication.Concentration,
                Category = medication.Category
            };

            // Agregar datos adicionales que no están en el ListViewModel
            ViewBag.CreatedDate = medication.CreatedDate.ToString("dd/MM/yyyy HH:mm");
            ViewBag.IsActive = medication.IsActive ? "Activo" : "Inactivo";

            return View(viewModel);
        }

        // GET: Medications/Create
        public IActionResult Create()
        {
            return View(new MedicationFormViewModel());
        }

        // POST: Medications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicationFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var medication = new Medication
                    {
                        MedicationName = model.MedicationName,
                        GenericName = model.GenericName,
                        Manufacturer = model.Manufacturer,
                        Concentration = model.Concentration,
                        Category = model.Category,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(medication);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Medicamento creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear medicamento");
                    ModelState.AddModelError("", "No se pudo guardar el medicamento. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: Medications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                return NotFound();
            }

            var model = new MedicationFormViewModel
            {
                MedicationId = medication.MedicationId,
                MedicationName = medication.MedicationName,
                GenericName = medication.GenericName,
                Manufacturer = medication.Manufacturer,
                Concentration = medication.Concentration,
                Category = medication.Category
            };

            return View(model);
        }

        // POST: Medications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicationFormViewModel model)
        {
            if (id != model.MedicationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var medication = await _context.Medications.FindAsync(id);
                    if (medication == null)
                    {
                        return NotFound();
                    }

                    medication.MedicationName = model.MedicationName;
                    medication.GenericName = model.GenericName;
                    medication.Manufacturer = model.Manufacturer;
                    medication.Concentration = model.Concentration;
                    medication.Category = model.Category;

                    _context.Update(medication);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Medicamento actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar medicamento");
                    ModelState.AddModelError("", "No se pudo actualizar el medicamento. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // POST: Medications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication != null)
            {
                medication.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Medicamento eliminado exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
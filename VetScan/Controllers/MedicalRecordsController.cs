// Controllers/MedicalRecordsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class MedicalRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalRecordsController> _logger;

        public MedicalRecordsController(ApplicationDbContext context, ILogger<MedicalRecordsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: MedicalRecords
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base sin ordenar inicialmente
            IQueryable<MedicalRecord> baseQuery = _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User);

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                baseQuery = baseQuery.Where(mr =>
                    mr.RecordNumber.Contains(searchString) ||
                    mr.Pet.PetName.Contains(searchString));
            }

            // Ordenar después de aplicar filtros
            var orderedQuery = baseQuery.OrderByDescending(mr => mr.CreationDate);

            var medicalRecords = await orderedQuery
                .Select(mr => new MedicalRecordListViewModel
                {
                    MedicalRecordId = mr.MedicalRecordId,
                    RecordNumber = mr.RecordNumber,
                    PetName = mr.Pet.PetName,
                    PetId = mr.PetId,
                    OwnerName = $"{mr.Pet.PetOwner.User.FirstName} {mr.Pet.PetOwner.User.LastName}",
                    CreationDate = mr.CreationDate,
                    Status = mr.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(medicalRecords);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .ThenInclude(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            var viewModel = new MedicalRecordListViewModel
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                RecordNumber = medicalRecord.RecordNumber,
                PetName = medicalRecord.Pet.PetName,
                PetId = medicalRecord.PetId,
                OwnerName = $"{medicalRecord.Pet.PetOwner.User.FirstName} {medicalRecord.Pet.PetOwner.User.LastName}",
                CreationDate = medicalRecord.CreationDate,
                Status = medicalRecord.Status
            };

            // Agregar datos adicionales que no están en el ListViewModel
            ViewBag.GeneralNotes = medicalRecord.GeneralNotes;
            ViewBag.PetCode = medicalRecord.Pet.PetCode;

            return View(viewModel);
        }

        // GET: MedicalRecords/Create
        public async Task<IActionResult> Create()
        {
            await LoadPetsViewData();
            return View(new MedicalRecordFormViewModel
            {
                RecordNumber = GenerateRecordNumber(),
                Status = "Active"
            });
        }

        // POST: MedicalRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecordFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = new MedicalRecord
                    {
                        PetId = model.PetId,
                        RecordNumber = model.RecordNumber,
                        GeneralNotes = model.GeneralNotes,
                        Status = model.Status,
                        CreationDate = DateTime.UtcNow
                    };

                    _context.Add(medicalRecord);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Registro médico creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear registro médico");
                    ModelState.AddModelError("", "No se pudo crear el registro médico. Intente nuevamente.");
                }
            }

            await LoadPetsViewData();
            return View(model);
        }

        // GET: MedicalRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            await LoadPetsViewData();

            var model = new MedicalRecordFormViewModel
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                PetId = medicalRecord.PetId,
                RecordNumber = medicalRecord.RecordNumber,
                GeneralNotes = medicalRecord.GeneralNotes,
                Status = medicalRecord.Status
            };

            ViewBag.PetName = medicalRecord.Pet.PetName;
            return View(model);
        }

        // POST: MedicalRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicalRecordFormViewModel model)
        {
            if (id != model.MedicalRecordId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var medicalRecord = await _context.MedicalRecords.FindAsync(id);
                    if (medicalRecord == null)
                    {
                        return NotFound();
                    }

                    medicalRecord.RecordNumber = model.RecordNumber;
                    medicalRecord.GeneralNotes = model.GeneralNotes;
                    medicalRecord.Status = model.Status;

                    _context.Update(medicalRecord);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Registro médico actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar registro médico");
                    ModelState.AddModelError("", "No se pudo actualizar el registro médico. Intente nuevamente.");
                }
            }

            await LoadPetsViewData();
            return View(model);
        }

        // GET: MedicalRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(mr => mr.Pet)
                .FirstOrDefaultAsync(mr => mr.MedicalRecordId == id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

        // POST: MedicalRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord != null)
            {
                _context.MedicalRecords.Remove(medicalRecord);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registro médico eliminado exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadPetsViewData()
        {
            ViewBag.Pets = await _context.Pets
                .Include(p => p.PetOwner)
                .ThenInclude(po => po.User)
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    PetId = p.PetId,
                    DisplayText = $"{p.PetName} (Dueño: {p.PetOwner.User.FirstName} {p.PetOwner.User.LastName})"
                })
                .ToListAsync();
        }

        private string GenerateRecordNumber()
        {
            return $"MR-{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
// Controllers/PrescriptionsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrescriptionsController> _logger;

        public PrescriptionsController(ApplicationDbContext context, ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Prescriptions
        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .OrderByDescending(p => p.CreatedDate)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p =>
                    p.Consultation.MedicalRecord.Pet.PetName.Contains(searchString) ||
                    p.Medication.MedicationName.Contains(searchString));
            }

            var prescriptions = await query
                .Select(p => new PrescriptionListViewModel
                {
                    PrescriptionId = p.PrescriptionId,
                    ConsultationId = p.ConsultationId,
                    ConsultationInfo = $"Consulta del {p.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = p.Consultation.MedicalRecord.Pet.PetName,
                    MedicationName = p.Medication.MedicationName,
                    Dosage = p.Dosage,
                    Frequency = p.Frequency,
                    CreatedDate = p.CreatedDate,
                    Status = p.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(prescriptions);
        }

        // GET: Prescriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            var viewModel = new PrescriptionListViewModel
            {
                PrescriptionId = prescription.PrescriptionId,
                ConsultationId = prescription.ConsultationId,
                ConsultationInfo = $"Consulta del {prescription.Consultation.ConsultationDate:dd/MM/yyyy}",
                PetName = prescription.Consultation.MedicalRecord.Pet.PetName,
                MedicationName = prescription.Medication.MedicationName,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                CreatedDate = prescription.CreatedDate,
                Status = prescription.Status
            };

            ViewBag.Duration = prescription.Duration;
            ViewBag.Instructions = prescription.Instructions;
            ViewBag.Quantity = prescription.Quantity;
            ViewBag.Refills = prescription.Refills;
            ViewBag.StartDate = prescription.StartDate?.ToString("dd/MM/yyyy");
            ViewBag.EndDate = prescription.EndDate?.ToString("dd/MM/yyyy");

            return View(viewModel);
        }

        // GET: Prescriptions/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new PrescriptionFormViewModel
            {
                StartDate = DateTime.Today,
                Status = "Active"
            });
        }

        // POST: Prescriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = new Prescription
                    {
                        ConsultationId = model.ConsultationId,
                        MedicationId = model.MedicationId,
                        Dosage = model.Dosage,
                        Frequency = model.Frequency,
                        Duration = model.Duration,
                        Instructions = model.Instructions,
                        Quantity = model.Quantity,
                        Refills = model.Refills,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        Status = model.Status,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.Add(prescription);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Prescripción creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear prescripción");
                    ModelState.AddModelError("", "No se pudo crear la prescripción. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Prescriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            var model = new PrescriptionFormViewModel
            {
                PrescriptionId = prescription.PrescriptionId,
                ConsultationId = prescription.ConsultationId,
                MedicationId = prescription.MedicationId,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                Duration = prescription.Duration,
                Instructions = prescription.Instructions,
                Quantity = prescription.Quantity,
                Refills = prescription.Refills,
                StartDate = prescription.StartDate,
                EndDate = prescription.EndDate,
                Status = prescription.Status
            };

            ViewBag.ConsultationInfo = $"Consulta del {prescription.Consultation.ConsultationDate:dd/MM/yyyy}";
            ViewBag.MedicationName = prescription.Medication.MedicationName;
            await LoadViewData();
            return View(model);
        }

        // POST: Prescriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PrescriptionFormViewModel model)
        {
            if (id != model.PrescriptionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var prescription = await _context.Prescriptions.FindAsync(id);
                    if (prescription == null)
                    {
                        return NotFound();
                    }

                    prescription.Dosage = model.Dosage;
                    prescription.Frequency = model.Frequency;
                    prescription.Duration = model.Duration;
                    prescription.Instructions = model.Instructions;
                    prescription.Quantity = model.Quantity;
                    prescription.Refills = model.Refills;
                    prescription.StartDate = model.StartDate;
                    prescription.EndDate = model.EndDate;
                    prescription.Status = model.Status;

                    _context.Update(prescription);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Prescripción actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar prescripción");
                    ModelState.AddModelError("", "No se pudo actualizar la prescripción. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Prescriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(p => p.Medication)
                .FirstOrDefaultAsync(m => m.PrescriptionId == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        // POST: Prescriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Prescripción eliminada exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewData()
        {
            // Cargar consultas médicas
            ViewBag.Consultations = await _context.MedicalConsultations
                .Include(c => c.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .OrderByDescending(c => c.ConsultationDate)
                .Select(c => new
                {
                    ConsultationId = c.ConsultationId,
                    DisplayText = $"{c.MedicalRecord.Pet.PetName} - {c.ConsultationDate:dd/MM/yyyy}"
                })
                .ToListAsync();

            // Cargar medicamentos activos
            ViewBag.Medications = await _context.Medications
                .Where(m => m.IsActive)
                .OrderBy(m => m.MedicationName)
                .Select(m => new
                {
                    MedicationId = m.MedicationId,
                    DisplayText = m.MedicationName
                })
                .ToListAsync();

            // Estados posibles
            ViewBag.StatusOptions = new List<string>
            {
                "Active",
                "Completed",
                "Cancelled",
                "Expired"
            };
        }
    }
}
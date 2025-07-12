// Controllers/VitalSignsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class VitalSignsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VitalSignsController> _logger;

        public VitalSignsController(ApplicationDbContext context, ILogger<VitalSignsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // Consulta base
            var query = _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .OrderByDescending(vs => vs.RecordedDate)
                .AsQueryable();

            // Aplicar filtro si hay término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(vs =>
                    vs.Consultation.MedicalRecord.Pet.PetName.Contains(searchString));
            }

            var vitalSigns = await query
                .Select(vs => new VitalSignListViewModel
                {
                    VitalSignId = vs.VitalSignId,
                    ConsultationId = vs.ConsultationId,
                    ConsultationInfo = $"Consulta del {vs.Consultation.ConsultationDate:dd/MM/yyyy}",
                    PetName = vs.Consultation.MedicalRecord.Pet.PetName,
                    RecordedDate = vs.RecordedDate,
                    Temperature = vs.Temperature,
                    HeartRate = vs.HeartRate,
                    RespiratoryRate = vs.RespiratoryRate,
                    Weight = vs.Weight,
                    BloodPressure = vs.BloodPressureSystolic.HasValue && vs.BloodPressureDiastolic.HasValue ?
                        $"{vs.BloodPressureSystolic}/{vs.BloodPressureDiastolic}" : "N/A"
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(vitalSigns);
        }

        // GET: VitalSigns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.AttendingVeterinarian)
                        .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

            var viewModel = new VitalSignListViewModel
            {
                VitalSignId = vitalSign.VitalSignId,
                ConsultationId = vitalSign.ConsultationId,
                ConsultationInfo = $"Consulta del {vitalSign.Consultation.ConsultationDate:dd/MM/yyyy}",
                PetName = vitalSign.Consultation.MedicalRecord.Pet.PetName,
                RecordedDate = vitalSign.RecordedDate,
                Temperature = vitalSign.Temperature,
                HeartRate = vitalSign.HeartRate,
                RespiratoryRate = vitalSign.RespiratoryRate,
                Weight = vitalSign.Weight,
                BloodPressure = vitalSign.BloodPressureSystolic.HasValue && vitalSign.BloodPressureDiastolic.HasValue ?
                    $"{vitalSign.BloodPressureSystolic}/{vitalSign.BloodPressureDiastolic}" : "N/A"
            };

            ViewBag.Notes = vitalSign.Notes;
            return View(viewModel);
        }

        // GET: VitalSigns/Create
        public async Task<IActionResult> Create()
        {
            await LoadConsultationsViewData();
            return View(new VitalSignFormViewModel
            {
                RecordedDate = DateTime.Now
            });
        }

        // POST: VitalSigns/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VitalSignFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var vitalSign = new VitalSign
                    {
                        ConsultationId = model.ConsultationId,
                        Temperature = model.Temperature,
                        HeartRate = model.HeartRate,
                        RespiratoryRate = model.RespiratoryRate,
                        Weight = model.Weight,
                        BloodPressureSystolic = model.BloodPressureSystolic,
                        BloodPressureDiastolic = model.BloodPressureDiastolic,
                        Notes = model.Notes,
                        RecordedDate = model.RecordedDate
                    };

                    _context.Add(vitalSign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Signos vitales registrados exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al registrar signos vitales");
                    ModelState.AddModelError("", "No se pudo registrar los signos vitales. Intente nuevamente.");
                }
            }

            await LoadConsultationsViewData();
            return View(model);
        }

        // GET: VitalSigns/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                .FirstOrDefaultAsync(vs => vs.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

            var model = new VitalSignFormViewModel
            {
                VitalSignId = vitalSign.VitalSignId,
                ConsultationId = vitalSign.ConsultationId,
                Temperature = vitalSign.Temperature,
                HeartRate = vitalSign.HeartRate,
                RespiratoryRate = vitalSign.RespiratoryRate,
                Weight = vitalSign.Weight,
                BloodPressureSystolic = vitalSign.BloodPressureSystolic,
                BloodPressureDiastolic = vitalSign.BloodPressureDiastolic,
                Notes = vitalSign.Notes,
                RecordedDate = vitalSign.RecordedDate
            };

            ViewBag.ConsultationInfo = $"Consulta del {vitalSign.Consultation.ConsultationDate:dd/MM/yyyy}";
            await LoadConsultationsViewData();
            return View(model);
        }

        // POST: VitalSigns/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VitalSignFormViewModel model)
        {
            if (id != model.VitalSignId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var vitalSign = await _context.VitalSigns.FindAsync(id);
                    if (vitalSign == null)
                    {
                        return NotFound();
                    }

                    vitalSign.Temperature = model.Temperature;
                    vitalSign.HeartRate = model.HeartRate;
                    vitalSign.RespiratoryRate = model.RespiratoryRate;
                    vitalSign.Weight = model.Weight;
                    vitalSign.BloodPressureSystolic = model.BloodPressureSystolic;
                    vitalSign.BloodPressureDiastolic = model.BloodPressureDiastolic;
                    vitalSign.Notes = model.Notes;
                    vitalSign.RecordedDate = model.RecordedDate;

                    _context.Update(vitalSign);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Signos vitales actualizados exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar signos vitales");
                    ModelState.AddModelError("", "No se pudo actualizar los signos vitales. Intente nuevamente.");
                }
            }

            await LoadConsultationsViewData();
            return View(model);
        }

        // GET: VitalSigns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vitalSign = await _context.VitalSigns
                .Include(vs => vs.Consultation)
                    .ThenInclude(c => c.MedicalRecord)
                        .ThenInclude(mr => mr.Pet)
                .FirstOrDefaultAsync(m => m.VitalSignId == id);

            if (vitalSign == null)
            {
                return NotFound();
            }

            return View(vitalSign);
        }

        // POST: VitalSigns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vitalSign = await _context.VitalSigns.FindAsync(id);
            if (vitalSign != null)
            {
                _context.VitalSigns.Remove(vitalSign);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Signos vitales eliminados exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadConsultationsViewData()
        {
            ViewBag.Consultations = await _context.MedicalConsultations
                .Include(c => c.MedicalRecord)
                    .ThenInclude(mr => mr.Pet)
                .OrderByDescending(c => c.ConsultationDate)
                .Select(c => new
                {
                    ConsultationId = c.ConsultationId,
                    DisplayText = $"{c.MedicalRecord.Pet.PetName} - {c.ConsultationDate:dd/MM/yyyy HH:mm}"
                })
                .ToListAsync();
        }
    }
}
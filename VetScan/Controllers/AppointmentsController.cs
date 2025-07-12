// AppointmentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            ApplicationDbContext context,
            ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Appointments
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .OrderByDescending(a => a.AppointmentDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a =>
                    a.Pet.PetName.Contains(searchString) ||
                    (a.Veterinarian.User.FirstName + " " + a.Veterinarian.User.LastName).Contains(searchString));
            }

            var appointments = await query
                .Select(a => new AppointmentListViewModel
                {
                    AppointmentId = a.AppointmentId,
                    PetName = a.Pet.PetName,
                    VeterinarianName = $"{a.Veterinarian.User.FirstName} {a.Veterinarian.User.LastName}",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentType = a.AppointmentType,
                    Status = a.Status
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(appointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var viewModel = new AppointmentListViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PetName = appointment.Pet.PetName,
                VeterinarianName = $"{appointment.Veterinarian.User.FirstName} {appointment.Veterinarian.User.LastName}",
                AppointmentDate = appointment.AppointmentDate,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status
            };

            ViewBag.Duration = appointment.Duration;
            ViewBag.Notes = appointment.Notes;
            ViewBag.ReasonForVisit = appointment.ReasonForVisit;
            ViewBag.EstimatedCost = appointment.EstimatedCost?.ToString("C");
            ViewBag.ActualCost = appointment.ActualCost?.ToString("C");

            return View(viewModel);
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View(new AppointmentFormViewModel
            {
                AppointmentDate = DateTime.Now
            });
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = new Appointment
                    {
                        PetId = model.PetId,
                        VeterinarianId = model.VeterinarianId,
                        AppointmentDate = model.AppointmentDate,
                        Duration = model.Duration,
                        AppointmentType = model.AppointmentType,
                        Status = model.Status,
                        Notes = model.Notes,
                        ReasonForVisit = model.ReasonForVisit,
                        EstimatedCost = model.EstimatedCost,
                        ActualCost = model.ActualCost
                    };

                    _context.Add(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cita creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear cita");
                    ModelState.AddModelError("", "No se pudo crear la cita. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var model = new AppointmentFormViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PetId = appointment.PetId,
                VeterinarianId = appointment.VeterinarianId,
                AppointmentDate = appointment.AppointmentDate,
                Duration = appointment.Duration,
                AppointmentType = appointment.AppointmentType,
                Status = appointment.Status,
                Notes = appointment.Notes,
                ReasonForVisit = appointment.ReasonForVisit,
                EstimatedCost = appointment.EstimatedCost,
                ActualCost = appointment.ActualCost
            };

            await LoadViewData();
            ViewBag.PetName = (await _context.Pets.FindAsync(appointment.PetId))?.PetName;
            ViewBag.VeterinarianName = (await _context.Veterinarians
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.VeterinarianId == appointment.VeterinarianId))?.User.FirstName;

            return View(model);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentFormViewModel model)
        {
            if (id != model.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var appointment = await _context.Appointments.FindAsync(id);
                    if (appointment == null)
                    {
                        return NotFound();
                    }

                    appointment.AppointmentDate = model.AppointmentDate;
                    appointment.Duration = model.Duration;
                    appointment.AppointmentType = model.AppointmentType;
                    appointment.Status = model.Status;
                    appointment.Notes = model.Notes;
                    appointment.ReasonForVisit = model.ReasonForVisit;
                    appointment.EstimatedCost = model.EstimatedCost;
                    appointment.ActualCost = model.ActualCost;

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cita actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar cita");
                    ModelState.AddModelError("", "No se pudo actualizar la cita. Intente nuevamente.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cita eliminada exitosamente";
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

            // Tipos de cita predefinidos
            ViewBag.AppointmentTypes = new List<string>
            {
                "Consulta General",
                "Vacunación",
                "Cirugía",
                "Control",
                "Emergencia",
                "Limpieza Dental",
                "Dermatología",
                "Oftalmología"
            };

            // Estados posibles
            ViewBag.StatusOptions = new List<string>
            {
                "Scheduled",
                "Completed",
                "Cancelled",
                "No Show"
            };
        }
    }
}
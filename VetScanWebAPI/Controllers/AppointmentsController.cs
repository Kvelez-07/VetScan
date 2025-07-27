// Controllers/AppointmentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ApplicationDbContext context, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Pet)
                    .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                    .Select(a => new AppointmentDto
                    {
                        AppointmentId = a.AppointmentId,
                        PetId = a.PetId,
                        PetName = a.Pet.PetName,
                        VeterinarianId = a.VeterinarianId,
                        VeterinarianName = $"{a.Veterinarian.User.FirstName} {a.Veterinarian.User.LastName}",
                        AppointmentDate = a.AppointmentDate,
                        Duration = a.Duration,
                        AppointmentType = a.AppointmentType,
                        Status = a.Status,
                        Notes = a.Notes,
                        ReasonForVisit = a.ReasonForVisit,
                        EstimatedCost = a.EstimatedCost,
                        ActualCost = a.ActualCost
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las citas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Pet)
                    .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                if (appointment == null)
                {
                    return NotFound();
                }

                var appointmentDto = new AppointmentDto
                {
                    AppointmentId = appointment.AppointmentId,
                    PetId = appointment.PetId,
                    PetName = appointment.Pet.PetName,
                    VeterinarianId = appointment.VeterinarianId,
                    VeterinarianName = $"{appointment.Veterinarian.User.FirstName} {appointment.Veterinarian.User.LastName}",
                    AppointmentDate = appointment.AppointmentDate,
                    Duration = appointment.Duration,
                    AppointmentType = appointment.AppointmentType,
                    Status = appointment.Status,
                    Notes = appointment.Notes,
                    ReasonForVisit = appointment.ReasonForVisit,
                    EstimatedCost = appointment.EstimatedCost,
                    ActualCost = appointment.ActualCost
                };

                return Ok(appointmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la cita con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> PostAppointment(AppointmentFormDto appointmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if pet exists
                if (!await _context.Pets.AnyAsync(p => p.PetId == appointmentDto.PetId && p.IsActive))
                {
                    return BadRequest("La mascota especificada no existe o no está activa");
                }

                // Check if veterinarian exists
                if (!await _context.Veterinarians.AnyAsync(v => v.VeterinarianId == appointmentDto.VeterinarianId))
                {
                    return BadRequest("El veterinario especificado no existe");
                }

                // Check for overlapping appointments
                var endTime = appointmentDto.AppointmentDate.AddMinutes(appointmentDto.Duration);
                var hasOverlap = await _context.Appointments
                    .AnyAsync(a => a.VeterinarianId == appointmentDto.VeterinarianId &&
                                 ((a.AppointmentDate <= appointmentDto.AppointmentDate &&
                                   a.AppointmentDate.AddMinutes(a.Duration) > appointmentDto.AppointmentDate) ||
                                  (a.AppointmentDate < endTime &&
                                   a.AppointmentDate.AddMinutes(a.Duration) >= endTime) ||
                                  (a.AppointmentDate >= appointmentDto.AppointmentDate &&
                                   a.AppointmentDate.AddMinutes(a.Duration) <= endTime)));

                if (hasOverlap)
                {
                    return Conflict("El veterinario ya tiene una cita programada en ese horario");
                }

                var appointment = new Appointment
                {
                    PetId = appointmentDto.PetId,
                    VeterinarianId = appointmentDto.VeterinarianId,
                    AppointmentDate = appointmentDto.AppointmentDate,
                    Duration = appointmentDto.Duration,
                    AppointmentType = appointmentDto.AppointmentType,
                    Status = appointmentDto.Status,
                    Notes = appointmentDto.Notes,
                    ReasonForVisit = appointmentDto.ReasonForVisit,
                    EstimatedCost = appointmentDto.EstimatedCost,
                    ActualCost = appointmentDto.ActualCost
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdAppointment = await _context.Appointments
                    .Include(a => a.Pet)
                    .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                var resultDto = new AppointmentDto
                {
                    AppointmentId = createdAppointment!.AppointmentId,
                    PetId = createdAppointment.PetId,
                    PetName = createdAppointment.Pet.PetName,
                    VeterinarianId = createdAppointment.VeterinarianId,
                    VeterinarianName = $"{createdAppointment.Veterinarian.User.FirstName} {createdAppointment.Veterinarian.User.LastName}",
                    AppointmentDate = createdAppointment.AppointmentDate,
                    Duration = createdAppointment.Duration,
                    AppointmentType = createdAppointment.AppointmentType,
                    Status = createdAppointment.Status,
                    Notes = createdAppointment.Notes,
                    ReasonForVisit = createdAppointment.ReasonForVisit,
                    EstimatedCost = createdAppointment.EstimatedCost,
                    ActualCost = createdAppointment.ActualCost
                };

                return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cita");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, AppointmentFormDto appointmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de cita inválido");
                }

                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                // Check if pet exists
                if (!await _context.Pets.AnyAsync(p => p.PetId == appointmentDto.PetId && p.IsActive))
                {
                    return BadRequest("La nueva mascota especificada no existe o no está activa");
                }

                // Check if veterinarian exists
                if (!await _context.Veterinarians.AnyAsync(v => v.VeterinarianId == appointmentDto.VeterinarianId))
                {
                    return BadRequest("El nuevo veterinario especificado no existe");
                }

                // Check for overlapping appointments (excluding current appointment)
                var endTime = appointmentDto.AppointmentDate.AddMinutes(appointmentDto.Duration);
                var hasOverlap = await _context.Appointments
                    .AnyAsync(a => a.VeterinarianId == appointmentDto.VeterinarianId &&
                                   a.AppointmentId != id &&
                                 ((a.AppointmentDate <= appointmentDto.AppointmentDate &&
                                   a.AppointmentDate.AddMinutes(a.Duration) > appointmentDto.AppointmentDate) ||
                                  (a.AppointmentDate < endTime &&
                                   a.AppointmentDate.AddMinutes(a.Duration) >= endTime) ||
                                  (a.AppointmentDate >= appointmentDto.AppointmentDate &&
                                   a.AppointmentDate.AddMinutes(a.Duration) <= endTime)));

                if (hasOverlap)
                {
                    return Conflict("El veterinario ya tiene una cita programada en ese horario");
                }

                appointment.PetId = appointmentDto.PetId;
                appointment.VeterinarianId = appointmentDto.VeterinarianId;
                appointment.AppointmentDate = appointmentDto.AppointmentDate;
                appointment.Duration = appointmentDto.Duration;
                appointment.AppointmentType = appointmentDto.AppointmentType;
                appointment.Status = appointmentDto.Status;
                appointment.Notes = appointmentDto.Notes;
                appointment.ReasonForVisit = appointmentDto.ReasonForVisit;
                appointment.EstimatedCost = appointmentDto.EstimatedCost;
                appointment.ActualCost = appointmentDto.ActualCost;

                _context.Entry(appointment).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar cita con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return NotFound();
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar cita con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }
    }
}
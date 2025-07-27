// Controllers/VeterinariansController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeterinariansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VeterinariansController> _logger;

        public VeterinariansController(ApplicationDbContext context, ILogger<VeterinariansController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Veterinarians
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VeterinarianDto>>> GetVeterinarians()
        {
            try
            {
                var veterinarians = await _context.Veterinarians
                    .Include(v => v.User)
                    .Include(v => v.Specialty)
                    .Include(v => v.MedicalConsultations)
                    .Include(v => v.Appointments)
                    .Select(v => new VeterinarianDto
                    {
                        VeterinarianId = v.VeterinarianId,
                        UserId = v.UserId,
                        Username = v.User.Username,
                        FirstName = v.User.FirstName,
                        LastName = v.User.LastName,
                        Email = v.User.Email,
                        SpecialtyId = v.SpecialtyId,
                        SpecialtyName = v.Specialty != null ? v.Specialty.SpecialtyName : null,
                        YearsOfExperience = v.YearsOfExperience,
                        Education = v.Education,
                        ConsultationCount = v.MedicalConsultations.Count,
                        AppointmentCount = v.Appointments.Count
                    })
                    .ToListAsync();

                return Ok(veterinarians);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los veterinarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Veterinarians/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VeterinarianDto>> GetVeterinarian(int id)
        {
            try
            {
                var veterinarian = await _context.Veterinarians
                    .Include(v => v.User)
                    .Include(v => v.Specialty)
                    .Include(v => v.MedicalConsultations)
                    .Include(v => v.Appointments)
                    .FirstOrDefaultAsync(v => v.VeterinarianId == id);

                if (veterinarian == null)
                {
                    return NotFound();
                }

                var veterinarianDto = new VeterinarianDto
                {
                    VeterinarianId = veterinarian.VeterinarianId,
                    UserId = veterinarian.UserId,
                    Username = veterinarian.User.Username,
                    FirstName = veterinarian.User.FirstName,
                    LastName = veterinarian.User.LastName,
                    Email = veterinarian.User.Email,
                    SpecialtyId = veterinarian.SpecialtyId,
                    SpecialtyName = veterinarian.Specialty != null ? veterinarian.Specialty.SpecialtyName : null,
                    YearsOfExperience = veterinarian.YearsOfExperience,
                    Education = veterinarian.Education,
                    ConsultationCount = veterinarian.MedicalConsultations.Count,
                    AppointmentCount = veterinarian.Appointments.Count
                };

                return Ok(veterinarianDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el veterinario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Veterinarians
        [HttpPost]
        public async Task<ActionResult<VeterinarianDto>> PostVeterinarian(VeterinarianFormDto veterinarianDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user exists
                var user = await _context.AppUsers.FindAsync(veterinarianDto.UserId);
                if (user == null)
                {
                    return BadRequest("El usuario especificado no existe");
                }

                // Check if user is already a veterinarian
                if (await _context.Veterinarians.AnyAsync(v => v.UserId == veterinarianDto.UserId))
                {
                    return Conflict("Este usuario ya está registrado como veterinario");
                }

                // Validate specialty if provided
                if (veterinarianDto.SpecialtyId.HasValue &&
                    !await _context.Specialties.AnyAsync(s => s.SpecialtyId == veterinarianDto.SpecialtyId.Value && s.IsActive))
                {
                    return BadRequest("La especialidad especificada no existe o no está activa");
                }

                var veterinarian = new Veterinarian
                {
                    UserId = veterinarianDto.UserId,
                    SpecialtyId = veterinarianDto.SpecialtyId,
                    YearsOfExperience = veterinarianDto.YearsOfExperience,
                    Education = veterinarianDto.Education
                };

                _context.Veterinarians.Add(veterinarian);
                await _context.SaveChangesAsync();

                // Reload with user and specialty info
                var createdVeterinarian = await _context.Veterinarians
                    .Include(v => v.User)
                    .Include(v => v.Specialty)
                    .FirstOrDefaultAsync(v => v.VeterinarianId == veterinarian.VeterinarianId);

                var resultDto = new VeterinarianDto
                {
                    VeterinarianId = createdVeterinarian!.VeterinarianId,
                    UserId = createdVeterinarian.UserId,
                    Username = createdVeterinarian.User.Username,
                    FirstName = createdVeterinarian.User.FirstName,
                    LastName = createdVeterinarian.User.LastName,
                    Email = createdVeterinarian.User.Email,
                    SpecialtyId = createdVeterinarian.SpecialtyId,
                    SpecialtyName = createdVeterinarian.Specialty != null ? createdVeterinarian.Specialty.SpecialtyName : null,
                    YearsOfExperience = createdVeterinarian.YearsOfExperience,
                    Education = createdVeterinarian.Education,
                    ConsultationCount = 0,
                    AppointmentCount = 0
                };

                return CreatedAtAction(nameof(GetVeterinarian), new { id = veterinarian.VeterinarianId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear veterinario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Veterinarians/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVeterinarian(int id, VeterinarianFormDto veterinarianDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de veterinario inválido");
                }

                var veterinarian = await _context.Veterinarians.FindAsync(id);
                if (veterinarian == null)
                {
                    return NotFound();
                }

                // Check if changing user ID
                if (veterinarian.UserId != veterinarianDto.UserId)
                {
                    // Verify new user exists
                    var userExists = await _context.AppUsers.AnyAsync(u => u.UserId == veterinarianDto.UserId);
                    if (!userExists)
                    {
                        return BadRequest("El nuevo usuario especificado no existe");
                    }

                    // Check if new user is already a veterinarian
                    if (await _context.Veterinarians.AnyAsync(v => v.UserId == veterinarianDto.UserId && v.VeterinarianId != id))
                    {
                        return Conflict("El nuevo usuario ya está registrado como veterinario");
                    }
                }

                // Validate specialty if provided
                if (veterinarianDto.SpecialtyId.HasValue &&
                    !await _context.Specialties.AnyAsync(s => s.SpecialtyId == veterinarianDto.SpecialtyId.Value && s.IsActive))
                {
                    return BadRequest("La especialidad especificada no existe o no está activa");
                }

                veterinarian.UserId = veterinarianDto.UserId;
                veterinarian.SpecialtyId = veterinarianDto.SpecialtyId;
                veterinarian.YearsOfExperience = veterinarianDto.YearsOfExperience;
                veterinarian.Education = veterinarianDto.Education;

                _context.Entry(veterinarian).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VeterinarianExists(id))
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
                _logger.LogError(ex, $"Error al actualizar veterinario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Veterinarians/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVeterinarian(int id)
        {
            try
            {
                var veterinarian = await _context.Veterinarians
                    .Include(v => v.MedicalConsultations)
                    .Include(v => v.Appointments)
                    .Include(v => v.VaccinationHistories)
                    .FirstOrDefaultAsync(v => v.VeterinarianId == id);

                if (veterinarian == null)
                {
                    return NotFound();
                }

                // Check if veterinarian has related records
                if (veterinarian.MedicalConsultations.Any() || veterinarian.Appointments.Any() || veterinarian.VaccinationHistories.Any())
                {
                    return BadRequest("No se puede eliminar un veterinario con consultas, citas o historiales de vacunación asociados");
                }

                _context.Veterinarians.Remove(veterinarian);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar veterinario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool VeterinarianExists(int id)
        {
            return _context.Veterinarians.Any(e => e.VeterinarianId == id);
        }
    }
}
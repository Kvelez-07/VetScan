// Controllers/VaccinationHistoriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinationHistoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccinationHistoriesController> _logger;

        public VaccinationHistoriesController(ApplicationDbContext context, ILogger<VaccinationHistoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/VaccinationHistories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VaccinationHistoryDto>>> GetVaccinationHistories()
        {
            try
            {
                var vaccinations = await _context.VaccinationHistories
                    .Include(v => v.Pet)
                    .Include(v => v.Vaccine)
                    .Include(v => v.Veterinarian)
                    .ThenInclude(v => v.User)
                    .Select(v => new VaccinationHistoryDto
                    {
                        VaccinationId = v.VaccinationId,
                        PetId = v.PetId,
                        PetName = v.Pet.PetName,
                        VaccineId = v.VaccineId,
                        VaccineName = v.Vaccine.VaccineName,
                        VeterinarianId = v.VeterinarianId,
                        VeterinarianName = $"{v.Veterinarian.User.FirstName} {v.Veterinarian.User.LastName}",
                        VaccinationDate = v.VaccinationDate,
                        BatchNumber = v.BatchNumber,
                        ExpirationDate = v.ExpirationDate,
                        NextDueDate = v.NextDueDate,
                        Reactions = v.Reactions
                    })
                    .ToListAsync();

                return Ok(vaccinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el historial de vacunación");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/VaccinationHistories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VaccinationHistoryDto>> GetVaccinationHistory(int id)
        {
            try
            {
                var vaccination = await _context.VaccinationHistories
                    .Include(v => v.Pet)
                    .Include(v => v.Vaccine)
                    .Include(v => v.Veterinarian)
                    .ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(v => v.VaccinationId == id);

                if (vaccination == null)
                {
                    return NotFound();
                }

                var vaccinationDto = new VaccinationHistoryDto
                {
                    VaccinationId = vaccination.VaccinationId,
                    PetId = vaccination.PetId,
                    PetName = vaccination.Pet.PetName,
                    VaccineId = vaccination.VaccineId,
                    VaccineName = vaccination.Vaccine.VaccineName,
                    VeterinarianId = vaccination.VeterinarianId,
                    VeterinarianName = $"{vaccination.Veterinarian.User.FirstName} {vaccination.Veterinarian.User.LastName}",
                    VaccinationDate = vaccination.VaccinationDate,
                    BatchNumber = vaccination.BatchNumber,
                    ExpirationDate = vaccination.ExpirationDate,
                    NextDueDate = vaccination.NextDueDate,
                    Reactions = vaccination.Reactions
                };

                return Ok(vaccinationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el registro de vacunación con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/VaccinationHistories
        [HttpPost]
        public async Task<ActionResult<VaccinationHistoryDto>> PostVaccinationHistory(VaccinationHistoryFormDto vaccinationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if pet exists
                if (!await _context.Pets.AnyAsync(p => p.PetId == vaccinationDto.PetId && p.IsActive))
                {
                    return BadRequest("La mascota especificada no existe o no está activa");
                }

                // Check if vaccine exists
                if (!await _context.Vaccines.AnyAsync(v => v.VaccineId == vaccinationDto.VaccineId))
                {
                    return BadRequest("La vacuna especificada no existe");
                }

                // Check if veterinarian exists
                if (!await _context.Veterinarians.AnyAsync(v => v.VeterinarianId == vaccinationDto.VeterinarianId))
                {
                    return BadRequest("El veterinario especificado no existe");
                }

                var vaccination = new VaccinationHistory
                {
                    PetId = vaccinationDto.PetId,
                    VaccineId = vaccinationDto.VaccineId,
                    VeterinarianId = vaccinationDto.VeterinarianId,
                    VaccinationDate = vaccinationDto.VaccinationDate,
                    BatchNumber = vaccinationDto.BatchNumber,
                    ExpirationDate = vaccinationDto.ExpirationDate,
                    NextDueDate = vaccinationDto.NextDueDate,
                    Reactions = vaccinationDto.Reactions
                };

                _context.VaccinationHistories.Add(vaccination);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdVaccination = await _context.VaccinationHistories
                    .Include(v => v.Pet)
                    .Include(v => v.Vaccine)
                    .Include(v => v.Veterinarian)
                    .ThenInclude(v => v.User)
                    .FirstOrDefaultAsync(v => v.VaccinationId == vaccination.VaccinationId);

                var resultDto = new VaccinationHistoryDto
                {
                    VaccinationId = createdVaccination!.VaccinationId,
                    PetId = createdVaccination.PetId,
                    PetName = createdVaccination.Pet.PetName,
                    VaccineId = createdVaccination.VaccineId,
                    VaccineName = createdVaccination.Vaccine.VaccineName,
                    VeterinarianId = createdVaccination.VeterinarianId,
                    VeterinarianName = $"{createdVaccination.Veterinarian.User.FirstName} {createdVaccination.Veterinarian.User.LastName}",
                    VaccinationDate = createdVaccination.VaccinationDate,
                    BatchNumber = createdVaccination.BatchNumber,
                    ExpirationDate = createdVaccination.ExpirationDate,
                    NextDueDate = createdVaccination.NextDueDate,
                    Reactions = createdVaccination.Reactions
                };

                return CreatedAtAction(nameof(GetVaccinationHistory), new { id = vaccination.VaccinationId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear registro de vacunación");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/VaccinationHistories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVaccinationHistory(int id, VaccinationHistoryFormDto vaccinationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de vacunación inválido");
                }

                var vaccination = await _context.VaccinationHistories.FindAsync(id);
                if (vaccination == null)
                {
                    return NotFound();
                }

                // Check if changing pet
                if (vaccination.PetId != vaccinationDto.PetId &&
                    !await _context.Pets.AnyAsync(p => p.PetId == vaccinationDto.PetId && p.IsActive))
                {
                    return BadRequest("La nueva mascota especificada no existe o no está activa");
                }

                // Check if changing vaccine
                if (vaccination.VaccineId != vaccinationDto.VaccineId &&
                    !await _context.Vaccines.AnyAsync(v => v.VaccineId == vaccinationDto.VaccineId))
                {
                    return BadRequest("La nueva vacuna especificada no existe");
                }

                // Check if changing veterinarian
                if (vaccination.VeterinarianId != vaccinationDto.VeterinarianId &&
                    !await _context.Veterinarians.AnyAsync(v => v.VeterinarianId == vaccinationDto.VeterinarianId))
                {
                    return BadRequest("El nuevo veterinario especificado no existe");
                }

                vaccination.PetId = vaccinationDto.PetId;
                vaccination.VaccineId = vaccinationDto.VaccineId;
                vaccination.VeterinarianId = vaccinationDto.VeterinarianId;
                vaccination.VaccinationDate = vaccinationDto.VaccinationDate;
                vaccination.BatchNumber = vaccinationDto.BatchNumber;
                vaccination.ExpirationDate = vaccinationDto.ExpirationDate;
                vaccination.NextDueDate = vaccinationDto.NextDueDate;
                vaccination.Reactions = vaccinationDto.Reactions;

                _context.Entry(vaccination).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VaccinationHistoryExists(id))
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
                _logger.LogError(ex, $"Error al actualizar registro de vacunación con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/VaccinationHistories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVaccinationHistory(int id)
        {
            try
            {
                var vaccination = await _context.VaccinationHistories.FindAsync(id);
                if (vaccination == null)
                {
                    return NotFound();
                }

                _context.VaccinationHistories.Remove(vaccination);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar registro de vacunación con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool VaccinationHistoryExists(int id)
        {
            return _context.VaccinationHistories.Any(e => e.VaccinationId == id);
        }
    }
}
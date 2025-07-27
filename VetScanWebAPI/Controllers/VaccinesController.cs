// Controllers/VaccinesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaccinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccinesController> _logger;

        public VaccinesController(ApplicationDbContext context, ILogger<VaccinesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Vaccines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VaccineDto>>> GetVaccines()
        {
            try
            {
                var vaccines = await _context.Vaccines
                    .Include(v => v.Species)
                    .Where(v => v.IsActive)
                    .Select(v => new VaccineDto
                    {
                        VaccineId = v.VaccineId,
                        VaccineName = v.VaccineName,
                        Manufacturer = v.Manufacturer,
                        VaccineType = v.VaccineType,
                        SpeciesId = v.SpeciesId,
                        SpeciesName = v.Species != null ? v.Species.SpeciesName : null,
                        RecommendedAge = v.RecommendedAge,
                        BoosterInterval = v.BoosterInterval,
                        IsCore = v.IsCore,
                        IsActive = v.IsActive,
                        CreatedDate = v.CreatedDate
                    })
                    .ToListAsync();

                return Ok(vaccines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener vacunas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Vaccines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VaccineDto>> GetVaccine(int id)
        {
            try
            {
                var vaccine = await _context.Vaccines
                    .Include(v => v.Species)
                    .FirstOrDefaultAsync(v => v.VaccineId == id && v.IsActive);

                if (vaccine == null)
                {
                    return NotFound();
                }

                var vaccineDto = new VaccineDto
                {
                    VaccineId = vaccine.VaccineId,
                    VaccineName = vaccine.VaccineName,
                    Manufacturer = vaccine.Manufacturer,
                    VaccineType = vaccine.VaccineType,
                    SpeciesId = vaccine.SpeciesId,
                    SpeciesName = vaccine.Species != null ? vaccine.Species.SpeciesName : null,
                    RecommendedAge = vaccine.RecommendedAge,
                    BoosterInterval = vaccine.BoosterInterval,
                    IsCore = vaccine.IsCore,
                    IsActive = vaccine.IsActive,
                    CreatedDate = vaccine.CreatedDate
                };

                return Ok(vaccineDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener vacuna con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Vaccines
        [HttpPost]
        public async Task<ActionResult<VaccineDto>> PostVaccine(VaccineFormDto vaccineDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if vaccine with same name already exists
                if (await _context.Vaccines.AnyAsync(v =>
                    v.VaccineName == vaccineDto.VaccineName))
                {
                    return Conflict("Ya existe una vacuna con ese nombre");
                }

                // Validate species exists if provided
                if (vaccineDto.SpeciesId.HasValue &&
                    !await _context.AnimalSpecies.AnyAsync(s => s.SpeciesId == vaccineDto.SpeciesId && s.IsActive))
                {
                    return BadRequest("La especie especificada no existe o no está activa");
                }

                var vaccine = new Vaccine
                {
                    VaccineName = vaccineDto.VaccineName,
                    Manufacturer = vaccineDto.Manufacturer,
                    VaccineType = vaccineDto.VaccineType,
                    SpeciesId = vaccineDto.SpeciesId,
                    RecommendedAge = vaccineDto.RecommendedAge,
                    BoosterInterval = vaccineDto.BoosterInterval,
                    IsCore = vaccineDto.IsCore,
                    IsActive = vaccineDto.IsActive,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Vaccines.Add(vaccine);
                await _context.SaveChangesAsync();

                // Reload with species info
                var createdVaccine = await _context.Vaccines
                    .Include(v => v.Species)
                    .FirstOrDefaultAsync(v => v.VaccineId == vaccine.VaccineId);

                var resultDto = new VaccineDto
                {
                    VaccineId = createdVaccine!.VaccineId,
                    VaccineName = createdVaccine.VaccineName,
                    Manufacturer = createdVaccine.Manufacturer,
                    VaccineType = createdVaccine.VaccineType,
                    SpeciesId = createdVaccine.SpeciesId,
                    SpeciesName = createdVaccine.Species != null ? createdVaccine.Species.SpeciesName : null,
                    RecommendedAge = createdVaccine.RecommendedAge,
                    BoosterInterval = createdVaccine.BoosterInterval,
                    IsCore = createdVaccine.IsCore,
                    IsActive = createdVaccine.IsActive,
                    CreatedDate = createdVaccine.CreatedDate
                };

                return CreatedAtAction(nameof(GetVaccine), new { id = vaccine.VaccineId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear vacuna");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Vaccines/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVaccine(int id, VaccineFormDto vaccineDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de vacuna inválido");
                }

                var vaccine = await _context.Vaccines.FindAsync(id);
                if (vaccine == null)
                {
                    return NotFound();
                }

                // Check if another vaccine has the same name
                if (await _context.Vaccines.AnyAsync(v =>
                    v.VaccineName == vaccineDto.VaccineName &&
                    v.VaccineId != id))
                {
                    return Conflict("Ya existe otra vacuna con ese nombre");
                }

                // Validate species exists if provided
                if (vaccineDto.SpeciesId.HasValue &&
                    !await _context.AnimalSpecies.AnyAsync(s => s.SpeciesId == vaccineDto.SpeciesId && s.IsActive))
                {
                    return BadRequest("La especie especificada no existe o no está activa");
                }

                vaccine.VaccineName = vaccineDto.VaccineName;
                vaccine.Manufacturer = vaccineDto.Manufacturer;
                vaccine.VaccineType = vaccineDto.VaccineType;
                vaccine.SpeciesId = vaccineDto.SpeciesId;
                vaccine.RecommendedAge = vaccineDto.RecommendedAge;
                vaccine.BoosterInterval = vaccineDto.BoosterInterval;
                vaccine.IsCore = vaccineDto.IsCore;
                vaccine.IsActive = vaccineDto.IsActive;

                _context.Entry(vaccine).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VaccineExists(id))
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
                _logger.LogError(ex, $"Error al actualizar vacuna con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Vaccines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVaccine(int id)
        {
            try
            {
                var vaccine = await _context.Vaccines.FindAsync(id);
                if (vaccine == null)
                {
                    return NotFound();
                }

                // Check if there are associated vaccination histories
                var hasHistories = await _context.VaccinationHistories
                    .AnyAsync(vh => vh.VaccineId == id);

                if (hasHistories)
                {
                    return BadRequest("No se puede eliminar la vacuna porque tiene historiales de vacunación asociados");
                }

                // Soft delete (mark as inactive)
                vaccine.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar vacuna con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool VaccineExists(int id)
        {
            return _context.Vaccines.Any(e => e.VaccineId == id);
        }
    }
}
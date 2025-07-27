// Controllers/SpecialtiesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SpecialtiesController> _logger;

        public SpecialtiesController(ApplicationDbContext context, ILogger<SpecialtiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Specialties
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpecialtyDto>>> GetSpecialties()
        {
            try
            {
                var specialties = await _context.Specialties
                    .Where(s => s.IsActive)
                    .Select(s => new SpecialtyDto
                    {
                        SpecialtyId = s.SpecialtyId,
                        SpecialtyName = s.SpecialtyName,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return Ok(specialties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especialidades");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Specialties/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SpecialtyDto>> GetSpecialty(int id)
        {
            try
            {
                var specialty = await _context.Specialties
                    .FirstOrDefaultAsync(s => s.SpecialtyId == id && s.IsActive);

                if (specialty == null)
                {
                    return NotFound();
                }

                var specialtyDto = new SpecialtyDto
                {
                    SpecialtyId = specialty.SpecialtyId,
                    SpecialtyName = specialty.SpecialtyName,
                    Description = specialty.Description,
                    IsActive = specialty.IsActive
                };

                return Ok(specialtyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener especialidad con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Specialties
        [HttpPost]
        public async Task<ActionResult<SpecialtyDto>> PostSpecialty(SpecialtyFormDto specialtyDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if specialty with same name already exists
                if (await _context.Specialties.AnyAsync(s =>
                    s.SpecialtyName == specialtyDto.SpecialtyName))
                {
                    return Conflict("Ya existe una especialidad con ese nombre");
                }

                var specialty = new Specialty
                {
                    SpecialtyName = specialtyDto.SpecialtyName,
                    Description = specialtyDto.Description,
                    IsActive = specialtyDto.IsActive
                };

                _context.Specialties.Add(specialty);
                await _context.SaveChangesAsync();

                var resultDto = new SpecialtyDto
                {
                    SpecialtyId = specialty.SpecialtyId,
                    SpecialtyName = specialty.SpecialtyName,
                    Description = specialty.Description,
                    IsActive = specialty.IsActive
                };

                return CreatedAtAction(nameof(GetSpecialty), new { id = specialty.SpecialtyId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear especialidad");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Specialties/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSpecialty(int id, SpecialtyFormDto specialtyDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de especialidad inválido");
                }

                var specialty = await _context.Specialties.FindAsync(id);
                if (specialty == null)
                {
                    return NotFound();
                }

                // Check if another specialty has the same name
                if (await _context.Specialties.AnyAsync(s =>
                    s.SpecialtyName == specialtyDto.SpecialtyName &&
                    s.SpecialtyId != id))
                {
                    return Conflict("Ya existe otra especialidad con ese nombre");
                }

                specialty.SpecialtyName = specialtyDto.SpecialtyName;
                specialty.Description = specialtyDto.Description;
                specialty.IsActive = specialtyDto.IsActive;

                _context.Entry(specialty).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpecialtyExists(id))
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
                _logger.LogError(ex, $"Error al actualizar especialidad con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Specialties/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            try
            {
                var specialty = await _context.Specialties.FindAsync(id);
                if (specialty == null)
                {
                    return NotFound();
                }

                // Check if there are associated veterinarians
                var hasVeterinarians = await _context.Veterinarians
                    .AnyAsync(v => v.SpecialtyId == id);

                if (hasVeterinarians)
                {
                    return BadRequest("No se puede eliminar la especialidad porque tiene veterinarios asociados");
                }

                // Soft delete (mark as inactive)
                specialty.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar especialidad con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool SpecialtyExists(int id)
        {
            return _context.Specialties.Any(e => e.SpecialtyId == id);
        }
    }
}
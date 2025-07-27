// Controllers/AnimalSpeciesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalSpeciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnimalSpeciesController> _logger;

        public AnimalSpeciesController(ApplicationDbContext context, ILogger<AnimalSpeciesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/AnimalSpecies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnimalSpeciesDto>>> GetAnimalSpecies()
        {
            try
            {
                var species = await _context.AnimalSpecies
                    .Where(s => s.IsActive)
                    .Select(s => new AnimalSpeciesDto
                    {
                        SpeciesId = s.SpeciesId,
                        SpeciesName = s.SpeciesName,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return Ok(species);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especies animales");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/AnimalSpecies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnimalSpeciesDto>> GetAnimalSpecies(int id)
        {
            try
            {
                var animalSpecies = await _context.AnimalSpecies.FindAsync(id);

                if (animalSpecies == null || !animalSpecies.IsActive)
                {
                    return NotFound();
                }

                var speciesDto = new AnimalSpeciesDto
                {
                    SpeciesId = animalSpecies.SpeciesId,
                    SpeciesName = animalSpecies.SpeciesName,
                    Description = animalSpecies.Description,
                    IsActive = animalSpecies.IsActive
                };

                return Ok(speciesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener especie animal con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/AnimalSpecies
        [HttpPost]
        public async Task<ActionResult<AnimalSpeciesDto>> PostAnimalSpecies(AnimalSpeciesFormDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar si ya existe una especie con el mismo nombre
                if (await _context.AnimalSpecies.AnyAsync(s => s.SpeciesName == createDto.SpeciesName))
                {
                    return Conflict("Ya existe una especie con ese nombre");
                }

                var animalSpecies = new AnimalSpecies
                {
                    SpeciesName = createDto.SpeciesName,
                    Description = createDto.Description,
                    IsActive = createDto.IsActive
                };

                _context.AnimalSpecies.Add(animalSpecies);
                await _context.SaveChangesAsync();

                var speciesDto = new AnimalSpeciesDto
                {
                    SpeciesId = animalSpecies.SpeciesId,
                    SpeciesName = animalSpecies.SpeciesName,
                    Description = animalSpecies.Description,
                    IsActive = animalSpecies.IsActive
                };

                return CreatedAtAction(nameof(GetAnimalSpecies), new { id = animalSpecies.SpeciesId }, speciesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear especie animal");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/AnimalSpecies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnimalSpecies(int id, AnimalSpeciesFormDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de especie inválido");
                }

                var animalSpecies = await _context.AnimalSpecies.FindAsync(id);

                if (animalSpecies == null)
                {
                    return NotFound();
                }

                // Verificar si otro registro tiene el mismo nombre
                if (await _context.AnimalSpecies
                    .AnyAsync(s => s.SpeciesName == updateDto.SpeciesName && s.SpeciesId != id))
                {
                    return Conflict("Ya existe otra especie con ese nombre");
                }

                animalSpecies.SpeciesName = updateDto.SpeciesName;
                animalSpecies.Description = updateDto.Description;
                animalSpecies.IsActive = updateDto.IsActive;

                _context.Entry(animalSpecies).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalSpeciesExists(id))
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
                _logger.LogError(ex, $"Error al actualizar especie animal con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/AnimalSpecies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnimalSpecies(int id)
        {
            try
            {
                var animalSpecies = await _context.AnimalSpecies.FindAsync(id);
                if (animalSpecies == null)
                {
                    return NotFound();
                }

                // Verificar si hay razas asociadas
                var hasBreeds = await _context.Breeds.AnyAsync(b => b.SpeciesId == id && b.IsActive);
                if (hasBreeds)
                {
                    return BadRequest("No se puede eliminar la especie porque tiene razas asociadas");
                }

                // Verificar si hay mascotas asociadas
                var hasPets = await _context.Pets.AnyAsync(p => p.SpeciesId == id && p.IsActive);
                if (hasPets)
                {
                    return BadRequest("No se puede eliminar la especie porque tiene mascotas asociadas");
                }

                // Verificar si hay vacunas asociadas
                var hasVaccines = await _context.Vaccines.AnyAsync(v => v.SpeciesId == id);
                if (hasVaccines)
                {
                    return BadRequest("No se puede eliminar la especie porque tiene vacunas asociadas");
                }

                // Soft delete (marcar como inactivo)
                animalSpecies.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar especie animal con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool AnimalSpeciesExists(int id)
        {
            return _context.AnimalSpecies.Any(e => e.SpeciesId == id);
        }
    }
}
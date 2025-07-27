// Controllers/BreedsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreedsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BreedsController> _logger;

        public BreedsController(ApplicationDbContext context, ILogger<BreedsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Breeds
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreedDto>>> GetBreeds()
        {
            try
            {
                var breeds = await _context.Breeds
                    .Include(b => b.Species)
                    .Where(b => b.IsActive)
                    .Select(b => new BreedDto
                    {
                        BreedId = b.BreedId,
                        SpeciesId = b.SpeciesId,
                        SpeciesName = b.Species.SpeciesName,
                        BreedName = b.BreedName,
                        Description = b.Description,
                        IsActive = b.IsActive
                    })
                    .ToListAsync();

                return Ok(breeds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener razas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Breeds/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BreedDto>> GetBreed(int id)
        {
            try
            {
                var breed = await _context.Breeds
                    .Include(b => b.Species)
                    .FirstOrDefaultAsync(b => b.BreedId == id && b.IsActive);

                if (breed == null)
                {
                    return NotFound();
                }

                var breedDto = new BreedDto
                {
                    BreedId = breed.BreedId,
                    SpeciesId = breed.SpeciesId,
                    SpeciesName = breed.Species.SpeciesName,
                    BreedName = breed.BreedName,
                    Description = breed.Description,
                    IsActive = breed.IsActive
                };

                return Ok(breedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener raza con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Breeds
        [HttpPost]
        public async Task<ActionResult<BreedDto>> PostBreed(BreedFormDto breedDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar que la especie exista y esté activa
                var speciesExists = await _context.AnimalSpecies
                    .AnyAsync(s => s.SpeciesId == breedDto.SpeciesId && s.IsActive);
                
                if (!speciesExists)
                {
                    return BadRequest("La especie especificada no existe o no está activa");
                }

                // Verificar si ya existe una raza con el mismo nombre para esta especie
                if (await _context.Breeds.AnyAsync(b => 
                    b.BreedName == breedDto.BreedName && 
                    b.SpeciesId == breedDto.SpeciesId))
                {
                    return Conflict("Ya existe una raza con ese nombre para esta especie");
                }

                var breed = new Breed
                {
                    SpeciesId = breedDto.SpeciesId,
                    BreedName = breedDto.BreedName,
                    Description = breedDto.Description,
                    IsActive = breedDto.IsActive
                };

                _context.Breeds.Add(breed);
                await _context.SaveChangesAsync();

                var createdBreed = await _context.Breeds
                    .Include(b => b.Species)
                    .FirstOrDefaultAsync(b => b.BreedId == breed.BreedId);

                var resultDto = new BreedDto
                {
                    BreedId = createdBreed!.BreedId,
                    SpeciesId = createdBreed.SpeciesId,
                    SpeciesName = createdBreed.Species.SpeciesName,
                    BreedName = createdBreed.BreedName,
                    Description = createdBreed.Description,
                    IsActive = createdBreed.IsActive
                };

                return CreatedAtAction(nameof(GetBreed), new { id = breed.BreedId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear raza");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Breeds/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBreed(int id, BreedFormDto breedDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de raza inválido");
                }

                var breed = await _context.Breeds.FindAsync(id);
                if (breed == null)
                {
                    return NotFound();
                }

                // Verificar que la especie exista y esté activa
                var speciesExists = await _context.AnimalSpecies
                    .AnyAsync(s => s.SpeciesId == breedDto.SpeciesId && s.IsActive);
                
                if (!speciesExists)
                {
                    return BadRequest("La especie especificada no existe o no está activa");
                }

                // Verificar si otra raza tiene el mismo nombre para esta especie
                if (await _context.Breeds.AnyAsync(b => 
                    b.BreedName == breedDto.BreedName && 
                    b.SpeciesId == breedDto.SpeciesId &&
                    b.BreedId != id))
                {
                    return Conflict("Ya existe otra raza con ese nombre para esta especie");
                }

                breed.SpeciesId = breedDto.SpeciesId;
                breed.BreedName = breedDto.BreedName;
                breed.Description = breedDto.Description;
                breed.IsActive = breedDto.IsActive;

                _context.Entry(breed).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BreedExists(id))
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
                _logger.LogError(ex, $"Error al actualizar raza con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Breeds/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBreed(int id)
        {
            try
            {
                var breed = await _context.Breeds.FindAsync(id);
                if (breed == null)
                {
                    return NotFound();
                }

                // Verificar si hay mascotas asociadas
                var hasPets = await _context.Pets.AnyAsync(p => p.BreedId == id && p.IsActive);
                if (hasPets)
                {
                    return BadRequest("No se puede eliminar la raza porque tiene mascotas asociadas");
                }

                // Soft delete (marcar como inactivo)
                breed.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar raza con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool BreedExists(int id)
        {
            return _context.Breeds.Any(e => e.BreedId == id);
        }
    }
}
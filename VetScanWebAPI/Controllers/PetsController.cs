// Controllers/PetsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PetsController> _logger;

        public PetsController(ApplicationDbContext context, ILogger<PetsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Pets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetDto>>> GetPets()
        {
            try
            {
                var pets = await _context.Pets
                    .Include(p => p.PetOwner)
                    .ThenInclude(po => po.User)
                    .Include(p => p.Species)
                    .Include(p => p.Breed)
                    .Select(p => new PetDto
                    {
                        PetId = p.PetId,
                        PetOwnerId = p.PetOwnerId,
                        PetOwnerName = $"{p.PetOwner.User.FirstName} {p.PetOwner.User.LastName}",
                        PetCode = p.PetCode,
                        PetName = p.PetName,
                        SpeciesId = p.SpeciesId,
                        SpeciesName = p.Species.SpeciesName,
                        BreedId = p.BreedId,
                        BreedName = p.Breed != null ? p.Breed.BreedName : null,
                        Gender = p.Gender,
                        DateOfBirth = p.DateOfBirth,
                        AgeDisplay = p.AgeDisplay,
                        Weight = p.Weight,
                        Color = p.Color,
                        IsActive = p.IsActive
                    })
                    .ToListAsync();

                return Ok(pets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las mascotas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/Pets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PetDto>> GetPet(int id)
        {
            try
            {
                var pet = await _context.Pets
                    .Include(p => p.PetOwner)
                    .ThenInclude(po => po.User)
                    .Include(p => p.Species)
                    .Include(p => p.Breed)
                    .FirstOrDefaultAsync(p => p.PetId == id);

                if (pet == null)
                {
                    return NotFound();
                }

                var petDto = new PetDto
                {
                    PetId = pet.PetId,
                    PetOwnerId = pet.PetOwnerId,
                    PetOwnerName = $"{pet.PetOwner.User.FirstName} {pet.PetOwner.User.LastName}",
                    PetCode = pet.PetCode,
                    PetName = pet.PetName,
                    SpeciesId = pet.SpeciesId,
                    SpeciesName = pet.Species.SpeciesName,
                    BreedId = pet.BreedId,
                    BreedName = pet.Breed != null ? pet.Breed.BreedName : null,
                    Gender = pet.Gender,
                    DateOfBirth = pet.DateOfBirth,
                    AgeDisplay = pet.AgeDisplay,
                    Weight = pet.Weight,
                    Color = pet.Color,
                    IsActive = pet.IsActive
                };

                return Ok(petDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener la mascota con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/Pets
        [HttpPost]
        public async Task<ActionResult<PetDto>> PostPet(PetFormDto petDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if pet owner exists
                if (!await _context.PetOwners.AnyAsync(po => po.PetOwnerId == petDto.PetOwnerId))
                {
                    return BadRequest("El dueño de mascota especificado no existe");
                }

                // Check if species exists
                if (!await _context.AnimalSpecies.AnyAsync(s => s.SpeciesId == petDto.SpeciesId && s.IsActive))
                {
                    return BadRequest("La especie especificada no existe o no está activa");
                }

                // Check if breed exists and belongs to species if provided
                if (petDto.BreedId.HasValue &&
                    !await _context.Breeds.AnyAsync(b => b.BreedId == petDto.BreedId.Value && b.IsActive))
                {
                    return BadRequest("La raza especificada no existe o no está activa");
                }

                // Check if pet code is unique
                if (await _context.Pets.AnyAsync(p => p.PetCode == petDto.PetCode))
                {
                    return Conflict("Ya existe una mascota con este código");
                }

                var pet = new Pet
                {
                    PetOwnerId = petDto.PetOwnerId,
                    PetCode = petDto.PetCode,
                    PetName = petDto.PetName,
                    SpeciesId = petDto.SpeciesId,
                    BreedId = petDto.BreedId,
                    Gender = petDto.Gender,
                    DateOfBirth = petDto.DateOfBirth,
                    Weight = petDto.Weight,
                    Color = petDto.Color,
                    IsActive = petDto.IsActive
                };

                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();

                // Reload with related data
                var createdPet = await _context.Pets
                    .Include(p => p.PetOwner)
                    .ThenInclude(po => po.User)
                    .Include(p => p.Species)
                    .Include(p => p.Breed)
                    .FirstOrDefaultAsync(p => p.PetId == pet.PetId);

                var resultDto = new PetDto
                {
                    PetId = createdPet!.PetId,
                    PetOwnerId = createdPet.PetOwnerId,
                    PetOwnerName = $"{createdPet.PetOwner.User.FirstName} {createdPet.PetOwner.User.LastName}",
                    PetCode = createdPet.PetCode,
                    PetName = createdPet.PetName,
                    SpeciesId = createdPet.SpeciesId,
                    SpeciesName = createdPet.Species.SpeciesName,
                    BreedId = createdPet.BreedId,
                    BreedName = createdPet.Breed != null ? createdPet.Breed.BreedName : null,
                    Gender = createdPet.Gender,
                    DateOfBirth = createdPet.DateOfBirth,
                    AgeDisplay = createdPet.AgeDisplay,
                    Weight = createdPet.Weight,
                    Color = createdPet.Color,
                    IsActive = createdPet.IsActive
                };

                return CreatedAtAction(nameof(GetPet), new { id = pet.PetId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear mascota");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/Pets/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPet(int id, PetFormDto petDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de mascota inválido");
                }

                var pet = await _context.Pets.FindAsync(id);
                if (pet == null)
                {
                    return NotFound();
                }

                // Check if changing pet owner
                if (pet.PetOwnerId != petDto.PetOwnerId &&
                    !await _context.PetOwners.AnyAsync(po => po.PetOwnerId == petDto.PetOwnerId))
                {
                    return BadRequest("El nuevo dueño de mascota no existe");
                }

                // Check if changing species
                if (pet.SpeciesId != petDto.SpeciesId &&
                    !await _context.AnimalSpecies.AnyAsync(s => s.SpeciesId == petDto.SpeciesId && s.IsActive))
                {
                    return BadRequest("La nueva especie no existe o no está activa");
                }

                // Check if changing breed
                if (pet.BreedId != petDto.BreedId && petDto.BreedId.HasValue &&
                    !await _context.Breeds.AnyAsync(b => b.BreedId == petDto.BreedId.Value && b.IsActive))
                {
                    return BadRequest("La nueva raza no existe o no está activa");
                }

                // Check if changing pet code to one that already exists
                if (pet.PetCode != petDto.PetCode &&
                    await _context.Pets.AnyAsync(p => p.PetCode == petDto.PetCode && p.PetId != id))
                {
                    return Conflict("Ya existe otra mascota con este código");
                }

                pet.PetOwnerId = petDto.PetOwnerId;
                pet.PetCode = petDto.PetCode;
                pet.PetName = petDto.PetName;
                pet.SpeciesId = petDto.SpeciesId;
                pet.BreedId = petDto.BreedId;
                pet.Gender = petDto.Gender;
                pet.DateOfBirth = petDto.DateOfBirth;
                pet.Weight = petDto.Weight;
                pet.Color = petDto.Color;
                pet.IsActive = petDto.IsActive;

                _context.Entry(pet).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetExists(id))
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
                _logger.LogError(ex, $"Error al actualizar mascota con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/Pets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(id);
                if (pet == null)
                {
                    return NotFound();
                }

                // Soft delete (update IsActive) might be better than physical delete
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar mascota con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool PetExists(int id)
        {
            return _context.Pets.Any(e => e.PetId == id);
        }
    }
}
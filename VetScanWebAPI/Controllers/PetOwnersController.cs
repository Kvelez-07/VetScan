// Controllers/PetOwnersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetOwnersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PetOwnersController> _logger;

        public PetOwnersController(ApplicationDbContext context, ILogger<PetOwnersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/PetOwners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PetOwnerDto>>> GetPetOwners()
        {
            try
            {
                var petOwners = await _context.PetOwners
                    .Include(po => po.User)
                    .Include(po => po.Pets)
                    .Select(po => new PetOwnerDto
                    {
                        PetOwnerId = po.PetOwnerId,
                        UserId = po.UserId,
                        Username = po.User.Username,
                        FirstName = po.User.FirstName,
                        LastName = po.User.LastName,
                        Email = po.User.Email,
                        PhoneNumber = po.User.PhoneNumber,
                        Address = po.Address,
                        City = po.City,
                        State = po.State,
                        PostalCode = po.PostalCode,
                        Country = po.Country,
                        EmergencyContactName = po.EmergencyContactName,
                        EmergencyContactPhone = po.EmergencyContactPhone,
                        PreferredContactMethod = po.PreferredContactMethod,
                        PetCount = po.Pets.Count
                    })
                    .ToListAsync();

                return Ok(petOwners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los dueños de mascotas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/PetOwners/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PetOwnerDto>> GetPetOwner(int id)
        {
            try
            {
                var petOwner = await _context.PetOwners
                    .Include(po => po.User)
                    .Include(po => po.Pets)
                    .FirstOrDefaultAsync(po => po.PetOwnerId == id);

                if (petOwner == null)
                {
                    return NotFound();
                }

                var petOwnerDto = new PetOwnerDto
                {
                    PetOwnerId = petOwner.PetOwnerId,
                    UserId = petOwner.UserId,
                    Username = petOwner.User.Username,
                    FirstName = petOwner.User.FirstName,
                    LastName = petOwner.User.LastName,
                    Email = petOwner.User.Email,
                    PhoneNumber = petOwner.User.PhoneNumber,
                    Address = petOwner.Address,
                    City = petOwner.City,
                    State = petOwner.State,
                    PostalCode = petOwner.PostalCode,
                    Country = petOwner.Country,
                    EmergencyContactName = petOwner.EmergencyContactName,
                    EmergencyContactPhone = petOwner.EmergencyContactPhone,
                    PreferredContactMethod = petOwner.PreferredContactMethod,
                    PetCount = petOwner.Pets.Count
                };

                return Ok(petOwnerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el dueño de mascota con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/PetOwners
        [HttpPost]
        public async Task<ActionResult<PetOwnerDto>> PostPetOwner(PetOwnerFormDto petOwnerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user exists
                var user = await _context.AppUsers.FindAsync(petOwnerDto.UserId);
                if (user == null)
                {
                    return BadRequest("El usuario especificado no existe");
                }

                // Check if user is already a pet owner
                if (await _context.PetOwners.AnyAsync(po => po.UserId == petOwnerDto.UserId))
                {
                    return Conflict("Este usuario ya está registrado como dueño de mascota");
                }

                var petOwner = new PetOwner
                {
                    UserId = petOwnerDto.UserId,
                    Address = petOwnerDto.Address,
                    City = petOwnerDto.City,
                    State = petOwnerDto.State,
                    PostalCode = petOwnerDto.PostalCode,
                    Country = petOwnerDto.Country,
                    EmergencyContactName = petOwnerDto.EmergencyContactName,
                    EmergencyContactPhone = petOwnerDto.EmergencyContactPhone,
                    PreferredContactMethod = petOwnerDto.PreferredContactMethod
                };

                _context.PetOwners.Add(petOwner);
                await _context.SaveChangesAsync();

                // Reload with user info
                var createdPetOwner = await _context.PetOwners
                    .Include(po => po.User)
                    .FirstOrDefaultAsync(po => po.PetOwnerId == petOwner.PetOwnerId);

                var resultDto = new PetOwnerDto
                {
                    PetOwnerId = createdPetOwner!.PetOwnerId,
                    UserId = createdPetOwner.UserId,
                    Username = createdPetOwner.User.Username,
                    FirstName = createdPetOwner.User.FirstName,
                    LastName = createdPetOwner.User.LastName,
                    Email = createdPetOwner.User.Email,
                    PhoneNumber = createdPetOwner.User.PhoneNumber,
                    Address = createdPetOwner.Address,
                    City = createdPetOwner.City,
                    State = createdPetOwner.State,
                    PostalCode = createdPetOwner.PostalCode,
                    Country = createdPetOwner.Country,
                    EmergencyContactName = createdPetOwner.EmergencyContactName,
                    EmergencyContactPhone = createdPetOwner.EmergencyContactPhone,
                    PreferredContactMethod = createdPetOwner.PreferredContactMethod,
                    PetCount = 0
                };

                return CreatedAtAction(nameof(GetPetOwner), new { id = petOwner.PetOwnerId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear dueño de mascota");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/PetOwners/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPetOwner(int id, PetOwnerFormDto petOwnerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de dueño de mascota inválido");
                }

                var petOwner = await _context.PetOwners.FindAsync(id);
                if (petOwner == null)
                {
                    return NotFound();
                }

                // Check if changing user ID
                if (petOwner.UserId != petOwnerDto.UserId)
                {
                    // Verify new user exists
                    var userExists = await _context.AppUsers.AnyAsync(u => u.UserId == petOwnerDto.UserId);
                    if (!userExists)
                    {
                        return BadRequest("El nuevo usuario especificado no existe");
                    }

                    // Check if new user is already a pet owner
                    if (await _context.PetOwners.AnyAsync(po => po.UserId == petOwnerDto.UserId && po.PetOwnerId != id))
                    {
                        return Conflict("El nuevo usuario ya está registrado como dueño de mascota");
                    }
                }

                petOwner.UserId = petOwnerDto.UserId;
                petOwner.Address = petOwnerDto.Address;
                petOwner.City = petOwnerDto.City;
                petOwner.State = petOwnerDto.State;
                petOwner.PostalCode = petOwnerDto.PostalCode;
                petOwner.Country = petOwnerDto.Country;
                petOwner.EmergencyContactName = petOwnerDto.EmergencyContactName;
                petOwner.EmergencyContactPhone = petOwnerDto.EmergencyContactPhone;
                petOwner.PreferredContactMethod = petOwnerDto.PreferredContactMethod;

                _context.Entry(petOwner).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PetOwnerExists(id))
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
                _logger.LogError(ex, $"Error al actualizar dueño de mascota con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/PetOwners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePetOwner(int id)
        {
            try
            {
                var petOwner = await _context.PetOwners
                    .Include(po => po.Pets)
                    .FirstOrDefaultAsync(po => po.PetOwnerId == id);

                if (petOwner == null)
                {
                    return NotFound();
                }

                // Check if pet owner has pets
                if (petOwner.Pets.Any())
                {
                    return BadRequest("No se puede eliminar un dueño de mascota que tiene mascotas registradas");
                }

                _context.PetOwners.Remove(petOwner);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar dueño de mascota con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool PetOwnerExists(int id)
        {
            return _context.PetOwners.Any(e => e.PetOwnerId == id);
        }
    }
}
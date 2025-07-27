// Controllers/AppUsersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppUsersController> _logger;

        public AppUsersController(ApplicationDbContext context, ILogger<AppUsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/AppUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUserDto>>> GetAppUsers()
        {
            try
            {
                var users = await _context.AppUsers
                    .Include(u => u.Role)
                    .Select(u => new AppUserDto
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        RoleName = u.Role.RoleName,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        PhoneNumber = u.PhoneNumber
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/AppUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUserDto>> GetAppUser(int id)
        {
            try
            {
                var user = await _context.AppUsers
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    return NotFound();
                }

                var userDto = new AppUserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    RoleName = user.Role.RoleName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener usuario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/AppUsers
        [HttpPost]
        public async Task<ActionResult<AppUserDto>> PostAppUser(AppUserFormDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if username already exists
                if (await _context.AppUsers.AnyAsync(u => u.Username == userDto.Username))
                {
                    return Conflict("Ya existe un usuario con ese nombre de usuario");
                }

                // Check if email already exists
                if (await _context.AppUsers.AnyAsync(u => u.Email == userDto.Email))
                {
                    return Conflict("Ya existe un usuario con ese correo electrónico");
                }

                // Validate role exists
                if (!await _context.UserRoles.AnyAsync(r => r.RoleId == userDto.RoleId && r.IsActive))
                {
                    return BadRequest("El rol especificado no existe o no está activo");
                }

                // In a real application, you would hash the password before storing it
                var user = new AppUser
                {
                    Username = userDto.Username,
                    Email = userDto.Email,
                    Password = userDto.Password, // Remember to hash this in production
                    RoleId = userDto.RoleId,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    PhoneNumber = userDto.PhoneNumber
                };

                _context.AppUsers.Add(user);
                await _context.SaveChangesAsync();

                // Reload with role info
                var createdUser = await _context.AppUsers
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                var resultDto = new AppUserDto
                {
                    UserId = createdUser!.UserId,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    RoleId = createdUser.RoleId,
                    RoleName = createdUser.Role.RoleName,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    PhoneNumber = createdUser.PhoneNumber
                };

                return CreatedAtAction(nameof(GetAppUser), new { id = user.UserId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/AppUsers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppUser(int id, AppUserFormDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de usuario inválido");
                }

                var user = await _context.AppUsers.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Check if another user has the same username
                if (await _context.AppUsers.AnyAsync(u =>
                    u.Username == userDto.Username &&
                    u.UserId != id))
                {
                    return Conflict("Ya existe otro usuario con ese nombre de usuario");
                }

                // Check if another user has the same email
                if (await _context.AppUsers.AnyAsync(u =>
                    u.Email == userDto.Email &&
                    u.UserId != id))
                {
                    return Conflict("Ya existe otro usuario con ese correo electrónico");
                }

                // Validate role exists
                if (!await _context.UserRoles.AnyAsync(r => r.RoleId == userDto.RoleId && r.IsActive))
                {
                    return BadRequest("El rol especificado no existe o no está activo");
                }

                user.Username = userDto.Username;
                user.Email = userDto.Email;

                // Only update password if it's provided (you might want a separate endpoint for password changes)
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.Password = userDto.Password; // Remember to hash this in production
                }

                user.RoleId = userDto.RoleId;
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.PhoneNumber = userDto.PhoneNumber;

                _context.Entry(user).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppUserExists(id))
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
                _logger.LogError(ex, $"Error al actualizar usuario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/AppUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppUser(int id)
        {
            try
            {
                var user = await _context.AppUsers.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // In a real application, you might want to implement soft delete
                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar usuario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool AppUserExists(int id)
        {
            return _context.AppUsers.Any(e => e.UserId == id);
        }
    }
}
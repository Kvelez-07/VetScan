// Controllers/UserRolesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(ApplicationDbContext context, ILogger<UserRolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/UserRoles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetUserRoles()
        {
            try
            {
                var roles = await _context.UserRoles
                    .Where(r => r.IsActive)
                    .Select(r => new UserRoleDto
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        Description = r.Description,
                        IsActive = r.IsActive
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles de usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/UserRoles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserRoleDto>> GetUserRole(int id)
        {
            try
            {
                var role = await _context.UserRoles
                    .FirstOrDefaultAsync(r => r.RoleId == id && r.IsActive);

                if (role == null)
                {
                    return NotFound();
                }

                var roleDto = new UserRoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    IsActive = role.IsActive
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener rol de usuario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/UserRoles
        [HttpPost]
        public async Task<ActionResult<UserRoleDto>> PostUserRole(UserRoleFormDto roleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if role with same name already exists
                if (await _context.UserRoles.AnyAsync(r =>
                    r.RoleName == roleDto.RoleName))
                {
                    return Conflict("Ya existe un rol con ese nombre");
                }

                var role = new UserRole
                {
                    RoleName = roleDto.RoleName,
                    Description = roleDto.Description,
                    IsActive = roleDto.IsActive
                };

                _context.UserRoles.Add(role);
                await _context.SaveChangesAsync();

                var resultDto = new UserRoleDto
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName,
                    Description = role.Description,
                    IsActive = role.IsActive
                };

                return CreatedAtAction(nameof(GetUserRole), new { id = role.RoleId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear rol de usuario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/UserRoles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserRole(int id, UserRoleFormDto roleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de rol inválido");
                }

                var role = await _context.UserRoles.FindAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Check if another role has the same name
                if (await _context.UserRoles.AnyAsync(r =>
                    r.RoleName == roleDto.RoleName &&
                    r.RoleId != id))
                {
                    return Conflict("Ya existe otro rol con ese nombre");
                }

                role.RoleName = roleDto.RoleName;
                role.Description = roleDto.Description;
                role.IsActive = roleDto.IsActive;

                _context.Entry(role).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserRoleExists(id))
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
                _logger.LogError(ex, $"Error al actualizar rol de usuario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/UserRoles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            try
            {
                var role = await _context.UserRoles.FindAsync(id);
                if (role == null)
                {
                    return NotFound();
                }

                // Check if there are associated users
                var hasUsers = await _context.AppUsers
                    .AnyAsync(u => u.RoleId == id);

                if (hasUsers)
                {
                    return BadRequest("No se puede eliminar el rol porque tiene usuarios asociados");
                }

                // Soft delete (mark as inactive)
                role.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar rol de usuario con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool UserRoleExists(int id)
        {
            return _context.UserRoles.Any(e => e.RoleId == id);
        }
    }
}
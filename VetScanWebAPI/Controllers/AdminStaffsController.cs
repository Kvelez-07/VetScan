// Controllers/AdminStaffsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScanWebAPI.Data;
using VetScanWebAPI.DTO;
using VetScanWebAPI.Models;

namespace VetScanWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminStaffsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminStaffsController> _logger;

        public AdminStaffsController(ApplicationDbContext context, ILogger<AdminStaffsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/AdminStaffs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminStaffDto>>> GetAdminStaffs()
        {
            try
            {
                var adminStaffs = await _context.AdminStaff
                    .Include(a => a.User)
                    .Select(a => new AdminStaffDto
                    {
                        AdminStaffId = a.AdminStaffId,
                        UserId = a.UserId,
                        Username = a.User.Username,
                        FirstName = a.User.FirstName,
                        LastName = a.User.LastName,
                        Email = a.User.Email,
                        Position = a.Position,
                        Department = a.Department,
                        HireDate = a.HireDate,
                        Salary = a.Salary
                    })
                    .ToListAsync();

                return Ok(adminStaffs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el personal administrativo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // GET: api/AdminStaffs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AdminStaffDto>> GetAdminStaff(int id)
        {
            try
            {
                var adminStaff = await _context.AdminStaff
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AdminStaffId == id);

                if (adminStaff == null)
                {
                    return NotFound();
                }

                var adminStaffDto = new AdminStaffDto
                {
                    AdminStaffId = adminStaff.AdminStaffId,
                    UserId = adminStaff.UserId,
                    Username = adminStaff.User.Username,
                    FirstName = adminStaff.User.FirstName,
                    LastName = adminStaff.User.LastName,
                    Email = adminStaff.User.Email,
                    Position = adminStaff.Position,
                    Department = adminStaff.Department,
                    HireDate = adminStaff.HireDate,
                    Salary = adminStaff.Salary
                };

                return Ok(adminStaffDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el personal administrativo con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // POST: api/AdminStaffs
        [HttpPost]
        public async Task<ActionResult<AdminStaffDto>> PostAdminStaff(AdminStaffFormDto adminStaffDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if user exists
                var user = await _context.AppUsers.FindAsync(adminStaffDto.UserId);
                if (user == null)
                {
                    return BadRequest("El usuario especificado no existe");
                }

                // Check if user is already admin staff
                if (await _context.AdminStaff.AnyAsync(a => a.UserId == adminStaffDto.UserId))
                {
                    return Conflict("Este usuario ya está registrado como personal administrativo");
                }

                var adminStaff = new AdminStaff
                {
                    UserId = adminStaffDto.UserId,
                    Position = adminStaffDto.Position,
                    Department = adminStaffDto.Department,
                    HireDate = adminStaffDto.HireDate,
                    Salary = adminStaffDto.Salary
                };

                _context.AdminStaff.Add(adminStaff);
                await _context.SaveChangesAsync();

                // Reload with user info
                var createdAdminStaff = await _context.AdminStaff
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.AdminStaffId == adminStaff.AdminStaffId);

                var resultDto = new AdminStaffDto
                {
                    AdminStaffId = createdAdminStaff!.AdminStaffId,
                    UserId = createdAdminStaff.UserId,
                    Username = createdAdminStaff.User.Username,
                    FirstName = createdAdminStaff.User.FirstName,
                    LastName = createdAdminStaff.User.LastName,
                    Email = createdAdminStaff.User.Email,
                    Position = createdAdminStaff.Position,
                    Department = createdAdminStaff.Department,
                    HireDate = createdAdminStaff.HireDate,
                    Salary = createdAdminStaff.Salary
                };

                return CreatedAtAction(nameof(GetAdminStaff), new { id = adminStaff.AdminStaffId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear personal administrativo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // PUT: api/AdminStaffs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdminStaff(int id, AdminStaffFormDto adminStaffDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    return BadRequest("ID de personal administrativo inválido");
                }

                var adminStaff = await _context.AdminStaff.FindAsync(id);
                if (adminStaff == null)
                {
                    return NotFound();
                }

                // Check if changing user ID
                if (adminStaff.UserId != adminStaffDto.UserId)
                {
                    // Verify new user exists
                    var userExists = await _context.AppUsers.AnyAsync(u => u.UserId == adminStaffDto.UserId);
                    if (!userExists)
                    {
                        return BadRequest("El nuevo usuario especificado no existe");
                    }

                    // Check if new user is already admin staff
                    if (await _context.AdminStaff.AnyAsync(a => a.UserId == adminStaffDto.UserId && a.AdminStaffId != id))
                    {
                        return Conflict("El nuevo usuario ya está registrado como personal administrativo");
                    }
                }

                adminStaff.UserId = adminStaffDto.UserId;
                adminStaff.Position = adminStaffDto.Position;
                adminStaff.Department = adminStaffDto.Department;
                adminStaff.HireDate = adminStaffDto.HireDate;
                adminStaff.Salary = adminStaffDto.Salary;

                _context.Entry(adminStaff).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminStaffExists(id))
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
                _logger.LogError(ex, $"Error al actualizar personal administrativo con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // DELETE: api/AdminStaffs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdminStaff(int id)
        {
            try
            {
                var adminStaff = await _context.AdminStaff.FindAsync(id);
                if (adminStaff == null)
                {
                    return NotFound();
                }

                _context.AdminStaff.Remove(adminStaff);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar personal administrativo con ID {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private bool AdminStaffExists(int id)
        {
            return _context.AdminStaff.Any(e => e.AdminStaffId == id);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetScan.Data;
using VetScan.Models;
using VetScan.ViewModels;

namespace VetScan.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(ApplicationDbContext context, ILogger<UserRolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: UserRoles
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.UserRoles
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(r =>
                    r.RoleName.Contains(searchString) ||
                    (r.Description != null && r.Description.Contains(searchString)));
            }

            var roles = await query
                .OrderBy(r => r.RoleName)
                .Select(r => new UserRoleListViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    UserCount = r.Users.Count
                })
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(roles);
        }

        // GET: UserRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (role == null)
            {
                return NotFound();
            }

            var viewModel = new UserRoleListViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                UserCount = role.Users.Count
            };

            return View(viewModel);
        }

        // GET: UserRoles/Create
        public IActionResult Create() => View();

        // POST: UserRoles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserRoleFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var role = new UserRole
                    {
                        RoleName = model.RoleName,
                        Description = model.Description,
                        IsActive = model.IsActive
                    };

                    _context.Add(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Rol creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al crear rol");
                    ModelState.AddModelError("", "No se pudo crear el rol. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: UserRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.UserRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new UserRoleFormViewModel
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive
            };

            return View(model);
        }

        // POST: UserRoles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserRoleFormViewModel model)
        {
            if (id != model.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var role = await _context.UserRoles.FindAsync(id);
                    if (role == null)
                    {
                        return NotFound();
                    }

                    role.RoleName = model.RoleName;
                    role.Description = model.Description;
                    role.IsActive = model.IsActive;

                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Rol actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar rol");
                    ModelState.AddModelError("", "No se pudo actualizar el rol. Intente nuevamente.");
                }
            }
            return View(model);
        }

        // GET: UserRoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _context.UserRoles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(m => m.RoleId == id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: UserRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.UserRoles.FindAsync(id);
            if (role != null)
            {
                try
                {
                    role.IsActive = false; // Soft delete
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Rol desactivado exitosamente";
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al desactivar rol");
                    TempData["ErrorMessage"] = "No se pudo desactivar el rol porque tiene usuarios asociados.";
                    return RedirectToAction(nameof(Delete), new { id });
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
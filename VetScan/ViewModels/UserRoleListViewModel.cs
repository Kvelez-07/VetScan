using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class UserRoleListViewModel
    {
        [Display(Name = "Id Rol")]
        public int RoleId { get; set; }

        [Display(Name = "Nombre del Rol")]
        [Required(ErrorMessage = "El nombre del rol es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string? Description { get; set; }

        [Display(Name = "Estado")]
        public bool IsActive { get; set; }

        [Display(Name = "Usuarios Asignados")]
        public int UserCount { get; set; }

        // Propiedades calculadas (sin annotations ya que no se usan en formularios)
        public string StatusClass => IsActive ? "status-active" : "status-inactive";
        public string StatusText => IsActive ? "Activo" : "Inactivo";
    }
}
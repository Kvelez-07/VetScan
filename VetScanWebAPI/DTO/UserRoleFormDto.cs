using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class UserRoleFormDto
    {
        [Required(ErrorMessage = "El nombre del rol es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre del rol no puede exceder 50 caracteres")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

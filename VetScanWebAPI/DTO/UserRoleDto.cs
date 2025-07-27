using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class UserRoleDto
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class AdminStaffDto
    {
        public int AdminStaffId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = "Administrador";

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = "General";

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        [Range(0, 999999.99)]
        public decimal Salary { get; set; }
    }
}

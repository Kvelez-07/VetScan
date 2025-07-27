using System.ComponentModel.DataAnnotations;

namespace VetScanWebAPI.DTO
{
    public class AdminStaffFormDto
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "El puesto es obligatorio")]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string Position { get; set; } = "Administrador";

        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        public string Department { get; set; } = "General";

        [Required(ErrorMessage = "La fecha de contratación es obligatoria")]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El salario es obligatorio")]
        [Range(0, 999999.99, ErrorMessage = "Debe ser entre 0 y 999999.99")]
        public decimal Salary { get; set; } = 0;
    }
}

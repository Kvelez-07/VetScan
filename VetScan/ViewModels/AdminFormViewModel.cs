using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class AdminFormViewModel
    {
        // Datos básicos del usuario (solo lectura)
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }

        // Datos específicos de AdminStaff
        [Required(ErrorMessage = "El puesto es obligatorio")]
        [StringLength(100)]
        public string Position { get; set; } = "Administrador";

        [Required(ErrorMessage = "El departamento es obligatorio")]
        [StringLength(100)]
        public string Department { get; set; } = "General";

        [Required(ErrorMessage = "El salario es obligatorio")]
        [Range(0, 999999.99, ErrorMessage = "El salario debe ser positivo")]
        public decimal Salary { get; set; } = 0;
    }
}
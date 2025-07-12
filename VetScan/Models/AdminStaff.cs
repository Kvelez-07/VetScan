using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class AdminStaff
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminStaffId { get; set; }

        [Required]
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
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 999999.99, ErrorMessage = "Debe ser entre 0 y 999999.99")]
        public decimal Salary { get; set; } = 0;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; } = null!;
    }
}
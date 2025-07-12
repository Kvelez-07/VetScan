using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetScan.Models
{
    public class MedicalConsultation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConsultationId { get; set; }

        [Required(ErrorMessage = "El registro médico es requerido")]
        [Display(Name = "Registro Médico")]
        public int MedicalRecordId { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        [Display(Name = "Veterinario")]
        public int VeterinarianId { get; set; }

        [Required(ErrorMessage = "La fecha de consulta es requerida")]
        [Display(Name = "Fecha de Consulta")]
        [DataType(DataType.DateTime)]
        public DateTime ConsultationDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de consulta es requerido")]
        [Display(Name = "Tipo de Consulta")]
        [StringLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        public string ConsultationType { get; set; } = "General";

        [Display(Name = "Diagnóstico")]
        [StringLength(1000, ErrorMessage = "No puede exceder 1000 caracteres")]
        public string? Diagnosis { get; set; }

        [Display(Name = "Tratamiento")]
        [StringLength(2000, ErrorMessage = "No puede exceder 2000 caracteres")]
        public string? Treatment { get; set; }

        [Display(Name = "Próxima Cita Recomendada")]
        [DataType(DataType.Date)]
        public DateTime? NextAppointmentRecommended { get; set; }

        [Display(Name = "Estado")]
        [StringLength(20)]
        public string Status { get; set; } = "Completed";

        // Navigation properties
        [ForeignKey("MedicalRecordId")]
        public virtual MedicalRecord MedicalRecord { get; set; } = null!;

        [ForeignKey("VeterinarianId")]
        public virtual Veterinarian AttendingVeterinarian { get; set; } = null!;

        public virtual ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
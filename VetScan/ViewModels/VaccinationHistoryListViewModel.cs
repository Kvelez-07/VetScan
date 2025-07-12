using System;
using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class VaccinationHistoryListViewModel
    {
        public int VaccinationId { get; set; }

        [Display(Name = "Mascota")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "Vacuna")]
        public string VaccineName { get; set; } = string.Empty;

        [Display(Name = "Veterinario")]
        public string VeterinarianName { get; set; } = string.Empty;

        [Display(Name = "Fecha de Vacunación")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime VaccinationDate { get; set; }

        [Display(Name = "Número de Lote")]
        public string BatchNumber { get; set; } = string.Empty;

        [Display(Name = "Próxima Dosis")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? NextDueDate { get; set; }

        // Propiedades para estilos
        public string NextDueStatusClass =>
            NextDueDate.HasValue && NextDueDate.Value < DateTime.Today ? "text-danger" : "text-success";

        public string NextDueStatusText =>
            NextDueDate.HasValue ?
                (NextDueDate.Value < DateTime.Today ? "Vencida" : "Pendiente") :
                "No aplica";
    }
}
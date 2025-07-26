using System.ComponentModel.DataAnnotations;

namespace VetScan.ViewModels
{
    public class VitalSignListViewModel
    {
        [Display(Name = "Id Signos Vitales")]
        public int VitalSignId { get; set; }

        [Display(Name = "Id Consulta")]
        public int ConsultationId { get; set; }

        [Display(Name = "Información de Consulta")]
        public string ConsultationInfo { get; set; } = string.Empty;

        [Display(Name = "Nombre de Mascota")]
        public string PetName { get; set; } = string.Empty;

        [Display(Name = "Fecha de Registro")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime RecordedDate { get; set; }

        [Display(Name = "Temperatura (°C)")]
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal? Temperature { get; set; }

        [Display(Name = "Frecuencia Cardíaca (lpm)")]
        public int? HeartRate { get; set; }

        [Display(Name = "Frecuencia Respiratoria (rpm)")]
        public int? RespiratoryRate { get; set; }

        [Display(Name = "Peso (kg)")]
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal? Weight { get; set; }

        [Display(Name = "Presión Arterial")]
        public string BloodPressure { get; set; } = string.Empty;

        // Propiedades calculadas (sin annotations ya que no se usan en formularios)
        public string FormattedDate => RecordedDate.ToString("dd/MM/yyyy HH:mm");
        public string FormattedTemperature => Temperature.HasValue ? $"{Temperature} °C" : "N/A";
        public string FormattedHeartRate => HeartRate.HasValue ? $"{HeartRate} lpm" : "N/A";
        public string FormattedRespiratoryRate => RespiratoryRate.HasValue ? $"{RespiratoryRate} rpm" : "N/A";
        public string FormattedWeight => Weight.HasValue ? $"{Weight} kg" : "N/A";
    }
}
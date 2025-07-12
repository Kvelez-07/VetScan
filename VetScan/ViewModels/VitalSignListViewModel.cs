// ViewModels/VitalSignListViewModel.cs
using System;

namespace VetScan.ViewModels
{
    public class VitalSignListViewModel
    {
        public int VitalSignId { get; set; }
        public int ConsultationId { get; set; }
        public string ConsultationInfo { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public DateTime RecordedDate { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Weight { get; set; }
        public string BloodPressure { get; set; } = string.Empty;

        // Propiedades calculadas
        public string FormattedDate => RecordedDate.ToString("dd/MM/yyyy HH:mm");
        public string FormattedTemperature => Temperature.HasValue ? $"{Temperature} °C" : "N/A";
        public string FormattedHeartRate => HeartRate.HasValue ? $"{HeartRate} lpm" : "N/A";
        public string FormattedRespiratoryRate => RespiratoryRate.HasValue ? $"{RespiratoryRate} rpm" : "N/A";
        public string FormattedWeight => Weight.HasValue ? $"{Weight} kg" : "N/A";
    }
}
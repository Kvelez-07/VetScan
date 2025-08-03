using System.Text.Json.Serialization;

namespace VetScan.Models
{
    public class EmailSettings
    {
        [JsonPropertyName("smtpServer")]
        public string SmtpServer { get; set; } = string.Empty;

        [JsonPropertyName("smtpPort")]
        public int SmtpPort { get; set; }

        [JsonPropertyName("smtpUsername")]
        public string SmtpUsername { get; set; } = string.Empty;

        [JsonPropertyName("smtpPassword")]
        public string SmtpPassword { get; set; } = string.Empty;

        [JsonPropertyName("fromEmail")]
        public string FromEmail { get; set; } = string.Empty;

        [JsonPropertyName("fromName")]
        public string FromName { get; set; } = string.Empty;
    }
}

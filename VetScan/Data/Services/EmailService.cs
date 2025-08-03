using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VetScan.Models;

namespace VetScan.Data.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogInformation("Attempting to send email to {Email} with subject {Subject}", email, subject);

            using var client = new SmtpClient();

            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                mimeMessage.To.Add(MailboxAddress.Parse(email));
                mimeMessage.Subject = subject;

                mimeMessage.Body = new TextPart("html") { Text = message };

                // Configuración de tiempo de espera y reintentos
                client.Timeout = 30000; // 30 segundos

                _logger.LogInformation("Connecting to SMTP server {Server}:{Port}",
                    _emailSettings.SmtpServer, _emailSettings.SmtpPort);

                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.SmtpPort,
                    SecureSocketOptions.StartTlsWhenAvailable);

                // Autenticación con manejo de errores más detallado
                if (!string.IsNullOrEmpty(_emailSettings.SmtpUsername))
                {
                    _logger.LogInformation("Authenticating with SMTP server");
                    await client.AuthenticateAsync(
                        _emailSettings.SmtpUsername,
                        _emailSettings.SmtpPassword);
                }

                _logger.LogInformation("Sending email");
                await client.SendAsync(mimeMessage);

                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Authentication failed for {Username}. Please verify: " +
                    "1. Correct username/password, " +
                    "2. App password is generated if using 2FA, " +
                    "3. Less secure apps is enabled if not using 2FA",
                    _emailSettings.SmtpUsername);
                throw new ApplicationException("Failed to authenticate with email server. Please check email settings.", authEx);
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP command error when sending email to {Email}. StatusCode: {StatusCode}",
                    email, smtpEx.StatusCode);
                throw new ApplicationException("Error sending email. SMTP server rejected the request.", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "General error sending email to {Email}", email);
                throw new ApplicationException("An error occurred while sending email.", ex);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP server");
                }
            }
        }
    }
}
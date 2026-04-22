using APPLICATION_LAYER.Models;
using APPLICATION_LAYER.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace APPLICATION_LAYER.Services.Implementations
{
    /// <summary>
    /// Email Service — sends transactional emails via MailKit/SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly Serilog.ILogger _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, Serilog.ILogger logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(string toEmail, string toName, string verificationUrl)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Verify your TripConnect email address";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;"">
                          <h2 style=""color:#2563eb;"">Hello {toName},</h2>
                          <p>Please verify your email address by clicking the button below.
                             This link expires in <strong>24 hours</strong>.</p>
                          <p style=""margin:24px 0;"">
                            <a href=""{verificationUrl}""
                               style=""background:#2563eb;color:#fff;padding:12px 24px;
                                      border-radius:6px;text-decoration:none;font-weight:600;"">
                              Verify Email
                            </a>
                          </p>
                          <p style=""color:#6b7280;font-size:13px;"">
                            If the button doesn't work, copy and paste this URL into your browser:<br/>
                            <a href=""{verificationUrl}"" style=""color:#2563eb;word-break:break-all;"">{verificationUrl}</a>
                          </p>
                          <p style=""color:#6b7280;font-size:13px;"">
                            If you did not request this, you can safely ignore this email.
                          </p>
                        </div>",
                    TextBody = $"Verify your TripConnect email: {verificationUrl} (expires in 24 hours)"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                var secureOption = _emailSettings.SmtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, secureOption);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.Information("Verification email sent to: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to send verification email to {Email}: {Message}", toEmail, ex.Message);
                throw;
            }
        }
    }
}

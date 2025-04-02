using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Escrow.Api.Infrastructure.Data.Configurations;
public class EmailConfiguration : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailConfiguration> _logger;

    public EmailConfiguration(IConfiguration configuration, ILogger<EmailConfiguration> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["MailConfiguration:SmtpHost"];
            var smtpPort = int.Parse(_configuration["MailConfiguration:SmtpPort"] ?? "587");
            var smtpUser = _configuration["MailConfiguration:SmtpUser"];
            var smtpPass = _configuration["MailConfiguration:SmtpPass"];
            var senderEmail = _configuration["MailConfiguration:SenderEmail"];

            // Validate sender email before using it
            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                _logger.LogError("Sender email is not configured properly.");
                return false;
            }

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail), // Ensure this is not null
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }

            _logger.LogInformation($"Email sent successfully to {to}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Email sending failed: {ex.Message}");
            return false;
        }
    }

}

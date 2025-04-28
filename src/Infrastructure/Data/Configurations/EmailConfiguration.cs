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

    private readonly string _smtpServer = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _fromEmail = "harshit.inventcolab@gmail.com";
    private readonly string _fromPassword = "xggpvgdhddkzyecb"; // Use App Password if 2FA enabled

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string name, string body)
    {
        try
        {
            var smtpHost = _smtpServer;
            var smtpPort = _smtpPort;
            var fromEmail = _fromEmail;
            var fromPassword = _fromPassword;

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
            {
                _logger.LogError("SMTP credentials are not configured.");
                return false;
            }

            string htmlTemplate = await File.ReadAllTextAsync("Templates/EmailTemplate.html");

            // Replace only the placeholders in the original template
            string htmlBody = htmlTemplate
                .Replace("{{subject}}", subject)
                .Replace("{{name}}", name)
                .Replace("{{toEmail}}", toEmail)
                .Replace("{{body}}", body);

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Email}", toEmail);
            return false;
        }
    }
}

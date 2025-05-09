using Escrow.Api.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;

public class EmailConfiguration : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailConfiguration> _logger;

    private readonly string? _smtpServer;
    private readonly int _smtpPort;
    private readonly string? _fromEmail;
    private readonly string? _fromPassword;
    private readonly string? _subject;

    public EmailConfiguration(IConfiguration configuration, ILogger<EmailConfiguration> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _smtpServer = _configuration["EmailSettings:SmtpServer"];
        _smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
        _fromEmail = _configuration["EmailSettings:FromEmail"];
        _fromPassword = _configuration["EmailSettings:FromPassword"];
        _subject = _configuration["EmailSettings:Subject"];
    }


    public async Task<bool> SendEmailAsync(string toEmail, string subject, string name, string body)
    {
        try
        {

            if (string.IsNullOrEmpty(_fromEmail) || string.IsNullOrEmpty(_fromPassword))
            {
                _logger.LogError("SMTP credentials are not configured.");
                return false;
            }

            string htmlTemplate = await File.ReadAllTextAsync("Templates/EmailTemplate.html");

            string htmlBody = htmlTemplate
                .Replace("{{subject}}", _subject)
                .Replace("{{name}}", name)
                .Replace("{{toEmail}}", toEmail)
                .Replace("{{body}}", body);

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);
            message.Bcc.Add(_fromEmail); // Optional BCC

            using var smtp = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_fromEmail, _fromPassword),
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

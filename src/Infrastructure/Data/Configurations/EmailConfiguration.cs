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

    //    public async Task<bool> SendEmailAsync(string toEmail, string subject, string name, string body)
    //    {
    //        try
    //        {
    //            var message = new MailMessage();
    //            message.From = new MailAddress(_fromEmail);
    //            message.To.Add(toEmail);
    //            message.Subject = "Contact-Us";

    //            var htmlBody = $@"<html lang=""en"">

    //<body style=""font-family: 'Lato', 'Merriweather', 'Roboto', sans-serif;"">
    //    <div className=""mainEmailWraper""
    //        style=""max-width: 680px; margin: 0 auto;border: 1.17px solid #D0D0D0;border-radius: 8px;"">
    //        <div className=""emailHeader""
    //            style=""padding: 10px;background-color: #110b2a;border-radius: 8px 8px 0 0;height: 70px;display: flex;align-items: center;justify-content: center;"">
    //            <div className=""logoOuter"" style=""text-align:center;"">
    //                <img src=""http://103.119.170.253:5000/UserUploads/09d77026-cb75-45f9-befb-de9d29f14a2b$$welink-logo-02%202.png""
    //                    alt=""Logo"" style=""width:100px;"" />
    //            </div>
    //        </div>

    //        <div className=""emailTempBody"" style="""">
    //            <div style=""padding: 25px 16px; background-color: #fff; gap: 16px;"">

    //                <p
    //                    style=""font-size: 18px;color: #000000;margin: 10px 0px;font-weight: 600;text-align: center;margin-bottom: 21px;"">
    //                    {{subject}}, </p>
    //                <h6 style=""font-size: 16px; font-weight: 400; color: #000000;margin: 0px; font-weight: 600; ""> Welcome
    //                    to the Escrow ! </h6>

    //                <p style=""color: #000000;font-size: 16px;margin: 10px 0px;"">Name: <strong
    //                        style=""font-size: 16px; color: #000000;margin: 10px 0px;font-weight: 600;"">{{name}}</strong></p>
    //                <p style=""color: #000000;font-size: 16px;margin: 10px 0px;"">Email: <strong
    //                        style=""font-size: 16px; color: #000000;margin: 10px 0px;font-weight: 600;""> {{toEmail}}</strong>
    //                </p>
    //                <p style=""color: #000000;font-size: 16px;margin: 10px 0px;"">Message:
    //                    <strong style=""font-size: 16px; color: #000000;margin: 10px 0px;font-weight: 600;""> {{body}} </strong>
    //                </p>
    //                <h6 style=""font-size: 16px; font-weight: 400; color: #000000;margin: 0px; "">Best regards,</h6>
    //                <p style=""font-size: 18px; color: #000000;margin: 5px 0px;font-weight: 600;""> Escrow</p>
    //            </div>
    //            <div>
    //                <h6 style=""
    //    font-size: 12px;
    //    color: #777777;
    //    text-align: center;
    //    margin: 0;
    //    margin-bottom: 15px;
    //""> This message was sent from your website contact form.</h6>
    //                <h6 style=""
    //    font-size: 12px;
    //    color: #777777;
    //    text-align: center;
    //    margin: 0;
    //    margin-bottom: 15px;
    //"">If you did not expect this, you can safely ignore it.</h6>

    //            </div>
    //        </div>

    //        <div style=""padding: 10px;font-size: 14px; background-color: #110b2a; color: #000; text-align:center;"">
    //            <div style=""font-size: 20px;font-weight: 600;color: #fff;margin-bottom: 10px;"">Get in touch</div>


    //            <ul style=""list-style: none; padding: 0; margin: 0; margin-top: 16px; text-align: center;"">
    //                <li style=""display: inline-block; margin:0 8px;"">
    //                    <a href=""https://mail.google.com/""
    //                        style=""display: flex; width: 40px; height: 40px; border-radius: 50px; background-color: #fff; position:relative;""
    //                        target=""_blank"">
    //                        <img src=""https://api-ap-south-mum-1.openstack.acecloudhosting.com:8080/invent-colab-obj-bucket/krat-easy/facility/pocket-chef_1743593658508.png""
    //                            alt=""Gmail"" style=""margin:auto;
    //position: absolute; inset: 0; filter: invert(100);"" />
    //                    </a>
    //                </li>
    //                <li style=""display: inline-block; margin:0 8px;"">
    //                    <a href=""https://www.linkedin.com/""
    //                        style=""display: flex; width: 40px; height: 40px; border-radius: 50px; background-color: #fff; position:relative;""
    //                        target=""_blank"">
    //                        <img src=https://api-ap-south-mum-1.openstack.acecloudhosting.com:8080/invent-colab-obj-bucket/krat-easy/facility/pocket-chef_1743593692469.png
    //                            }} alt=""LinkedIn"" style=""margin:auto;position: absolute; inset: 0; filter: invert(100);"" />
    //                    </a>
    //                </li>
    //                <li style=""display: inline-block; margin:0 8px;"">
    //                    <a href=""https://twitter.com/""
    //                        style=""display: flex; width: 40px; height: 40px; border-radius: 50px; background-color: #fff; position:relative;""
    //                        target=""_blank"">
    //                        <img src=https://api-ap-south-mum-1.openstack.acecloudhosting.com:8080/invent-colab-obj-bucket/krat-easy/facility/pocket-chef_1743593784793.png
    //                            }} alt=""Twitter"" style=""margin:auto;position: absolute; inset: 0; filter: invert(100);"" />
    //                    </a>
    //                </li>
    //                <li style=""display: inline-block; margin:0 8px;"">
    //                    <a href=""https://www.facebook.com/""
    //                        style=""display: flex; width: 40px; height: 40px; border-radius: 50px; background-color: #fff; position:relative;""
    //                        target=""_blank"">
    //                        <img src=https://api-ap-south-mum-1.openstack.acecloudhosting.com:8080/invent-colab-obj-bucket/krat-easy/facility/pocket-chef_1743593729983.png
    //                            }} alt=""Facebook"" style=""margin:auto;position: absolute; inset: 0; filter: invert(100);"" />
    //                    </a>
    //                </li>
    //            </ul>
    //        </div>
    //        <div className=""emailFooter""
    //            style=""padding: 10px;background-color: #110b2a;border-radius: 0 0 8px 8px;text-align: center;"">
    //            <div className=""title"" style=""font-size: 14px; color: #fff; font-weight: 500;"">Copyright © 2024 Escrow.
    //                All rights reserved.</div>
    //        </div>
    //    </div>
    //</body>

    //</html>";







    //            //    var htmlBody = $@"
    //            //<html>
    //            //<head>
    //            //    <style>
    //            //        body {{
    //            //            font-family: Arial, sans-serif;
    //            //            color: #333333;
    //            //            margin: 0;
    //            //            padding: 0;
    //            //        }}
    //            //        .email-container {{
    //            //            background-color: #f4f4f4;
    //            //            padding: 20px;
    //            //            border-radius: 5px;
    //            //            width: 100%;
    //            //            max-width: 600px;
    //            //            margin: 0 auto;
    //            //        }}
    //            //        .header {{
    //            //            background-color: #4CAF50;
    //            //            color: white;
    //            //            padding: 15px;
    //            //            text-align: center;
    //            //            border-radius: 5px 5px 0 0;
    //            //        }}
    //            //        .content {{
    //            //            padding: 20px;
    //            //            background-color: white;
    //            //            border-radius: 0 0 5px 5px;
    //            //        }}
    //            //        .footer {{
    //            //            text-align: center;
    //            //            font-size: 12px;
    //            //            color: #777777;
    //            //            margin-top: 20px;
    //            //        }}
    //            //        a {{
    //            //            color: #4CAF50;
    //            //            text-decoration: none;
    //            //        }}
    //            //    </style>
    //            //</head>
    //            //<body>
    //            //    <div class='email-container'>
    //            //        <div class='header'>
    //            //            <h2>{subject}</h2>
    //            //        </div>
    //            //        <div class='content'>
    //            //            <p>You have received a new message via the Contact Us form.</p>
    //            //            <p><strong>Name:</strong> {name}</p>
    //            //            <p><strong>Email:</strong> {toEmail}</p>
    //            //            <p><strong>Message:</strong><br>{body}</p>
    //            //        </div>
    //            //        <div class='footer'>
    //            //            <p>This message was sent from your website contact form.</p>
    //            //            <p>If you did not expect this, you can safely ignore it.</p>
    //            //        </div>
    //            //    </div>
    //            //</body>
    //            //</html>";

    //            message.Body = htmlBody;
    //            message.IsBodyHtml = true;

    //            using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
    //            {
    //                smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
    //                smtp.EnableSsl = true;

    //                await smtp.SendMailAsync(message);
    //            }

    //            return true;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError($"Failed to send email: {ex.Message}");
    //            return false;
    //        }
    //    }

    //public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    //{
    //    try
    //    {
    //        var message = new MailMessage();
    //        message.From = new MailAddress(_fromEmail);
    //        message.To.Add(toEmail);
    //        message.Subject = "Contact-Us";

    //        // Create an HTML body with basic structure
    //        var htmlBody = $@"
    //        <html>
    //        <head>
    //            <style>
    //                body {{
    //                    font-family: Arial, sans-serif;
    //                    color: #333333;
    //                    margin: 0;
    //                    padding: 0;
    //                }}
    //                .email-container {{
    //                    background-color: #f4f4f4;
    //                    padding: 20px;
    //                    border-radius: 5px;
    //                    width: 100%;
    //                    max-width: 600px;
    //                    margin: 0 auto;
    //                }}
    //                .header {{
    //                    background-color: #4CAF50;
    //                    color: white;
    //                    padding: 15px;
    //                    text-align: center;
    //                    border-radius: 5px 5px 0 0;
    //                }}
    //                .content {{
    //                    padding: 20px;
    //                    background-color: white;
    //                    border-radius: 0 0 5px 5px;
    //                }}
    //                .footer {{
    //                    text-align: center;
    //                    font-size: 12px;
    //                    color: #777777;
    //                    margin-top: 20px;
    //                }}
    //                a {{
    //                    color: #4CAF50;
    //                    text-decoration: none;
    //                }}
    //            </style>
    //        </head>
    //        <body>
    //            <div class='email-container'>
    //                <div class='header'>
    //                    <h2>{subject}</h2>
    //                </div>
    //                <div class='content'>
    //                    <p>{body}</p>
    //                </div>
    //                <div class='footer'>
    //                    <p>Sent from your trusted service.</p>
    //                    <p>If you did not request this email, please ignore it.</p>
    //                </div>
    //            </div>
    //        </body>
    //        </html>";

    //        message.Body = htmlBody;
    //        message.IsBodyHtml = true; // Ensure the body is in HTML format

    //        using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
    //        {
    //            smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
    //            smtp.EnableSsl = true;

    //            await smtp.SendMailAsync(message);
    //        }

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log error
    //        _logger.LogError($"Failed to send email: {ex.Message}");
    //        return false;
    //    }
    //}




    //public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    //{
    //    try
    //    {
    //        var message = new MailMessage();
    //        message.From = new MailAddress(_fromEmail);
    //        message.To.Add(toEmail);
    //        message.Subject = subject;
    //        message.Body = body;
    //        message.IsBodyHtml = true; // Set to false if plain text

    //        using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
    //        {
    //            smtp.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
    //            smtp.EnableSsl = true;

    //            await smtp.SendMailAsync(message);
    //        }

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log error
    //        Console.WriteLine($"Failed to send email: {ex.Message}");
    //        return false;
    //    }
    //}


    //public async Task<bool> SendEmailAsync(string to, string subject, string body)
    //{
    //    try
    //    {
    //        var smtpHost = _configuration["MailConfiguration:SmtpHost"];
    //        var smtpPort = int.Parse(_configuration["MailConfiguration:SmtpPort"] ?? "587");
    //        var smtpUser = _configuration["MailConfiguration:SmtpUser"];
    //        var smtpPass = _configuration["MailConfiguration:SmtpPass"];
    //        var senderEmail = _configuration["MailConfiguration:SenderEmail"];

    //        // Validate sender email before using it
    //        if (string.IsNullOrWhiteSpace(senderEmail))
    //        {
    //            _logger.LogError("Sender email is not configured properly.");
    //            return false;
    //        }

    //        using (var client = new SmtpClient(smtpHost, smtpPort))
    //        {
    //            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
    //            client.EnableSsl = true;

    //            var mailMessage = new MailMessage
    //            {
    //                From = new MailAddress(senderEmail), // Ensure this is not null
    //                Subject = subject,
    //                Body = body,
    //                IsBodyHtml = false
    //            };
    //            mailMessage.To.Add(to);

    //            await client.SendMailAsync(mailMessage);
    //        }

    //        _logger.LogInformation($"Email sent successfully to {to}");
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError($"Email sending failed: {ex.Message}");
    //        return false;
    //    }
    //}

}

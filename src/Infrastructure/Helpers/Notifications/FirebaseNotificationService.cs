using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Escrow.Api.Infrastructure.Helpers.Notifications; // Replace with your real namespace


namespace Escrow.Api.Infrastructure.Helpers.Notifications;

public class FirebaseNotificationService : INotificationService
{
    private static bool _isInitialized = false;
    private readonly ILogger<FirebaseNotificationService> _logger;

    public FirebaseNotificationService(IConfiguration configuration, ILogger<FirebaseNotificationService> logger)
    {
        _logger = logger;

        if (!_isInitialized)
        {
            var path = configuration["Firebase:CredentialsPath"];
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path)
            });
            _isInitialized = true;
        }
    }

    public async Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body, object? data = null)
    {
        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data?.ToDictionary()
            };

            var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Push notification sent: {Result}", result);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Firebase push notification.");
            return false;
        }
    }
}

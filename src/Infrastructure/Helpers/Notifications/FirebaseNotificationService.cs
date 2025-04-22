using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Escrow.Api.Application.Common.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Escrow.Api.Infrastructure.Helpers.Notifications
{
    public class FirebaseNotificationService : INotificationService
    {
        private static bool _isInitialized = false;
        private readonly ILogger<FirebaseNotificationService> _logger;

        public FirebaseNotificationService(IConfiguration configuration, ILogger<FirebaseNotificationService> logger)
        {
            _logger = logger;

            if (!_isInitialized)
            {
                try
                {
                    var path = configuration["Firebase:CredentialsPath"];
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new InvalidOperationException("Firebase credentials path is not configured.");
                    }

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });

                    _isInitialized = true;
                    _logger.LogInformation("FirebaseApp initialized successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing FirebaseApp.");
                    throw;
                }
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
                    Data = data?.ToDictionary() ?? new Dictionary<string, string>()
                };

                var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Push notification sent. Result: {Result}", result);

                return true;
            }
            catch (FirebaseMessagingException fcmEx)
            {
                _logger.LogError(fcmEx, "Firebase messaging error: {Error}", fcmEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending push notification.");
                return false;
            }
        }
    }
}






























//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Escrow.Api.Application.Common.Interfaces;
//using FirebaseAdmin;
//using FirebaseAdmin.Messaging;
//using Google.Apis.Auth.OAuth2;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Escrow.Api.Infrastructure.Helpers.Notifications; // Replace with your real namespace


//namespace Escrow.Api.Infrastructure.Helpers.Notifications;

//public class FirebaseNotificationService : INotificationService
//{
//    private static bool _isInitialized = false;
//    private readonly ILogger<FirebaseNotificationService> _logger;

//    public FirebaseNotificationService(IConfiguration configuration, ILogger<FirebaseNotificationService> logger)
//    {
//        _logger = logger;

//        if (!_isInitialized)
//        {
//            var path = configuration["Firebase:CredentialsPath"];
//            FirebaseApp.Create(new AppOptions
//            {
//                Credential = GoogleCredential.FromFile(path)
//            });
//            _isInitialized = true;
//        }
//    }

//    public async Task<bool> SendPushNotificationAsync(string deviceToken, string title, string body, object? data = null)
//    {
//        //FirebaseApp.Create(new AppOptions
//        //{
//        //    Credential = GoogleCredential.FromFile("C:/Users/navee/OneDrive/Desktop/New folder/escrow_backend/src/Web/wwwroot/firebase/escrow-service-account.json")
//        //});
//        try
//        {
//            var message = new Message
//            {
//                Token = deviceToken,
//                Notification = new Notification
//                {
//                    Title = title,
//                    Body = body
//                },
//                Data = data?.ToDictionary()
//            };

//            var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
//            _logger.LogInformation("Push notification sent: {message}", message);

//            return true;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error sending Firebase push notification.");
//            return false;
//        }
//    }
//}

namespace Olive.PushNotification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Olive;
    using Newtonsoft.Json.Linq;
    using PushSharp.Apple;
    using PushSharp.Google;
    using PushSharp.Windows;
    using PushSharp.Common;

    /// <summary>
    /// Sends push notifications to ios, android and windows devices
    /// </summary>
    public class PushNotificationService
    {
        static ApnsServiceBroker ApnsBroker; // Apple service broker
        static GcmServiceBroker GcmBroker; // Google service broker
        static WnsServiceBroker WnsBroker; // Windows service broker

        static void InitializeBrokers()
        {
            InitializeAppleBroker();
            InitializeGoogleBroker();
            InitializeWindowsBroker();
        }

        static ApnsConfiguration CreateAppleConfig()
        {
            var certFile = AppDomain.CurrentDomain.WebsiteRoot().GetFile(Config.Get("PushNotification:Apple:CertificateFile")).FullName;

            return new ApnsConfiguration(
                Config.Get<ApnsConfiguration.ApnsServerEnvironment>("PushNotification:Apple:Environment"),
                certFile,
                Config.Get("PushNotification:Apple:CertificatePassword"));
        }

        static void InitializeAppleBroker()
        {
            if (ApnsBroker != null) return;
            if (!Config.IsDefined("PushNotification:Apple:CertificateFile")) return;

            // Configuration (NOTE: .pfx can also be used here)
            var config = CreateAppleConfig();

            // Create a new broker
            ApnsBroker = new ApnsServiceBroker(config);

            // Wire up events
            ApnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    LogError(ApnsBroker, ex);
                    return true; // Mark it as handled
                });
            };

            // Start the broker
            ApnsBroker.Start();
        }

        static void InitializeGoogleBroker()
        {
            if (GcmBroker != null) return;

            if (!Config.IsDefined("PushNotification:Google:SenderId")) return;

            var config = new GcmConfiguration(Config.Get("PushNotification:Google:SenderId"), Config.Get("PushNotification:Google:AuthToken"), null)
            { GcmUrl = "https://fcm.googleapis.com/fcm/send" };

            GcmBroker = new GcmServiceBroker(config);

            GcmBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    LogError(GcmBroker, ex);
                    return true; // Mark it as handled
                });
            };

            GcmBroker.OnNotificationSucceeded += notification =>
            {
                Olive.Log.Debug("Notification has been sent successfully: " + notification);
            };

            GcmBroker.Start();
        }

        static void InitializeWindowsBroker()
        {
            if (WnsBroker != null) return;
            if (!Config.IsDefined("PushNotification:Windows:PackageName")) return;

            // Configuration
            var config = new WnsConfiguration(Config.Get("PushNotification:Windows:PackageName"),
                                          Config.Get("PushNotification:Windows:PackageSID"),
                                          Config.Get("PushNotification:Windows:ClientSecret"));

            WnsBroker = new WnsServiceBroker(config);

            WnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    LogError(WnsBroker, ex);
                    return true; // Mark it as handled
                });
            };

            WnsBroker.Start();
        }

        public static void Send(string messageTitle, string messageBody, IEnumerable<IUserDevice> devices)
        {
            InitializeBrokers();

            // Send to iOS devices
            if (ApnsBroker != null)
                devices.Where(d => d.DeviceType == "iOS").Do(d =>
                {
                    ApnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = d.PushNotificationToken,
                        Payload = JObject.FromObject(new { aps = new { alert = new { title = messageTitle, body = messageBody } } })
                    });
                });

            // Send to Android devices
            if (GcmBroker != null)
            {
                var androidDevices = devices.Where(d => d.DeviceType == "Android").Select(d => d.PushNotificationToken).ToList();
                if (androidDevices.Any())
                    GcmBroker.QueueNotification(new GcmNotification
                    {
                        RegistrationIds = androidDevices, // This is for multicast messages
                        Notification = JObject.FromObject(new { body = messageBody, title = messageTitle }),
                        Data = JObject.FromObject(new { body = messageBody, title = messageTitle })
                    });
            }

            // Send to Windows devices
            if (WnsBroker != null)
            {
                foreach (var uri in devices.Where(d => d.DeviceType == "Windows").Select(d => d.PushNotificationToken))
                {
                    // Queue a notification to send
                    WnsBroker.QueueNotification(new WnsToastNotification
                    {
                        ChannelUri = uri,
                        Payload = new XElement("toast", new XElement("visual", new XElement("binding", new XAttribute("template", "ToastText01"), new XElement("text", new XAttribute("id", "1"), messageBody))))
                    });
                }
            }
        }

        /// <summary>
        /// Updates badge number of user's devices
        /// </summary>
        /// <param name="badge">0 for removing badge icon</param>
        public static void UpdateBadge(int badge, IEnumerable<IUserDevice> devices)
        {
            InitializeBrokers();

            // Send to iOS devices
            if (ApnsBroker != null)
            {
                devices.Where(d => d.DeviceType == "iOS").Do(d =>
                {
                    // Queue a notification to send
                    ApnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = d.PushNotificationToken,
                        Payload = JObject.FromObject(new { aps = new { badge = badge } })
                    });
                });
            }

            // TODO: Add for the other platfoorms.
        }

        // /// <summary>
        // /// Finds the expired tokens. (Should be called frequently to avoid sending redundant messages to expired devices)
        // /// </summary>
        // public static string[] GetExpiredTokens()
        // {
        //    var service = new FeedbackService(CreateAppleConfig());
        //    service.FeedbackReceived += (string deviceToken, DateTime timestamp) =>
        //    {

        //    };
        //    service.Check();

        //    // Mark android tokens
        //    //TODO: Find and mark expired android devices

        //    // Mark windows tokens
        //    //TODO: Find and mark expired windows devices

        // }

        static void LogError(object broker, Exception ex)
        {
            var description = broker.GetType().Name + " - Push notification failed.";

            if (ex is ApnsNotificationException)
            {
                var error = ex as ApnsNotificationException;
                description += $"ID={error.Notification.Identifier}, Code={error.ErrorStatusCode}";
            }

            if (ex is GcmNotificationException)
            {
                var error = (GcmNotificationException)ex;
                description += $"ID={error.Notification.MessageId}, Desc={error.Description}";
            }
            else if (ex is GcmMulticastResultException)
            {
                var error = (GcmMulticastResultException)ex;

                description += error.Succeeded.Select(x => "ID=" + x.MessageId).ToString(", ").WithWrappers("\r\n Succeeded:{", "}");

                description += error.Failed.Select(x => "ID=" + x.Key.MessageId + ", Desc=" + x.Value.Message)
                    .ToString(", ").WithWrappers("\r\n Failed:{", "}");
            }
            else if (ex is DeviceSubscriptionExpiredException)
            {
                var error = (DeviceSubscriptionExpiredException)ex;

                description += $"Device RegistrationId Expired: {error.OldSubscriptionId}";

                description += error.NewSubscriptionId.WithPrefix("\r\nDevice RegistrationId Changed To:");
            }
            else if (ex is RetryAfterException error)
            {
                // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                description += $"GCM Rate Limited, don't send more until after {error.RetryAfterUtc}";
            }

            Olive.Log.Error(description, ex);
        }

        /// <summary>
        /// Stops and unassigns all brokers
        /// </summary>
        public static void StopBrokers()
        {
            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            ApnsBroker.Stop();
            GcmBroker.Stop();
            WnsBroker.Stop();

            ApnsBroker = null;
            GcmBroker = null;
            WnsBroker = null;
        }
    }
}

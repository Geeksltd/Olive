namespace Olive.PushNotification
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Olive;
    using PushSharp.Apple;
    using PushSharp.Common;
    using PushSharp.Google;
    using PushSharp.Windows;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class PushNotificationService : IPushNotificationService
    {
        readonly ILogger<PushNotificationService> Logger;
        readonly ISubscriptionIdResolver Resolver;
        ApnsServiceBroker ApnsBroker; // Apple service broker
        GcmServiceBroker GcmBroker; // Google service broker
        WnsServiceBroker WnsBroker; // Windows service broker

        public PushNotificationService(ILogger<PushNotificationService> logger, ISubscriptionIdResolver resolver)
        {
            Logger = logger;
            Resolver = resolver;
        }

        public bool Send(string messageTitle, string messageBody, IEnumerable<IUserDevice> devices)
        {
            try
            {
                InitializeBrokers();

                // Send to iOS devices
                if (ApnsBroker != null)
                {
                    devices.Where(d => d.DeviceType == "iOS").Do(d =>
                    {
                        ApnsBroker.QueueNotification(new ApnsNotification
                        {
                            DeviceToken = d.PushNotificationToken,
                            Payload = JObject.FromObject(new { aps = new { alert = new { title = messageTitle, body = messageBody } } })
                        });
                    });
                }

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

                return !(ApnsBroker == null && GcmBroker == null && WnsBroker == null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                return false;
            }
        }

        public bool UpdateBadge(int badge, IEnumerable<IUserDevice> devices)
        {
            try
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

                return ApnsBroker is not null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);

                return false;
            }
        }

        void InitializeBrokers()
        {
            InitializeAppleBroker();
            InitializeGoogleBroker();
            InitializeWindowsBroker();
        }

        void InitializeAppleBroker()
        {
            if (ApnsBroker != null) return;

            if (Config.Get("PushNotification:Apple:CertificateFile").IsEmpty()) return;

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

        ApnsConfiguration CreateAppleConfig()
        {
            var certFile = AppDomain.CurrentDomain.WebsiteRoot().GetFile(Config.Get("PushNotification:Apple:CertificateFile")).FullName;

            return new ApnsConfiguration(
                Config.Get<ApnsConfiguration.ApnsServerEnvironment>("PushNotification:Apple:Environment"),
                certFile,
                Config.Get("PushNotification:Apple:CertificatePassword"));
        }

        void InitializeGoogleBroker()
        {
            if (GcmBroker != null) return;

            if (Config.Get("PushNotification:Google:SenderId").IsEmpty()) return;

            var config = new GcmConfiguration(Config.Get("PushNotification:Google:SenderId"),
                Config.Get("PushNotification:Google:AuthToken"), null)
            { GcmUrl = "https://fcm.googleapis.com/fcm/send" };

            GcmBroker = new GcmServiceBroker(config);

            GcmBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    if (ex is DeviceSubscriptionExpiredException expiredException)
                        Resolver.ResolveExpiredSubscription(expiredException.OldSubscriptionId, expiredException.NewSubscriptionId);

                    LogError(GcmBroker, ex);
                    return true; // Mark it as handled
                });
            };

            GcmBroker.OnNotificationSucceeded += notification =>
            {
                Logger.Debug("Notification has been sent successfully: " + notification);
            };

            GcmBroker.Start();
        }

        void InitializeWindowsBroker()
        {
            if (WnsBroker != null) return;
            if (Config.Get("PushNotification:Windows:PackageName").IsEmpty()) return;

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

        void LogError(object broker, Exception ex)
        {
            var description = broker.GetType().Name + " - Push notification failed.";

            if (ex is ApnsNotificationException)
            {
                var error = ex as ApnsNotificationException;
                description += $"ID={error.Notification.Identifier}, Code={error.ErrorStatusCode}";
            }

            if (ex is GcmNotificationException notificationError)
            {
                description += $"ID={notificationError.Notification.MessageId}, Desc={notificationError.Description}";
            }
            else if (ex is GcmMulticastResultException multiCastError)
            {
                description += multiCastError.Succeeded.Select(x => "ID=" + x.MessageId).ToString(", ").WithWrappers("\r\n Succeeded:{", "}");

                description += multiCastError.Failed.Select(x => "ID=" + x.Key.MessageId + ", Desc=" + x.Value.Message)
                    .ToString(", ").WithWrappers("\r\n Failed:{", "}");
            }
            else if (ex is DeviceSubscriptionExpiredException error)
            {
                description += $"Device RegistrationId Expired: {error.OldSubscriptionId}";
                description += error.NewSubscriptionId.WithPrefix("\r\nDevice RegistrationId Changed To:");
            }
            else if (ex is RetryAfterException raeError)
            {
                // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                description += $"GCM Rate Limited, don't send more until after {raeError.RetryAfterUtc}";
            }

            Logger.Error(ex, description);
        }
    }
}

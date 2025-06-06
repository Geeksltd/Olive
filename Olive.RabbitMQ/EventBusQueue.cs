﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Olive.RabbitMQ
{
    public class EventBusQueue : IEventBusQueue
    {
        const int MAX_RETRY = 4;
        const int BATCH_SIZE = 100;
        internal string QueueUrl;
        public IChannel Client { get; set; }
        public ConnectionFactory Factory { get; set; }
        internal bool IsFifo => QueueUrl.EndsWith(".fifo");
        readonly Limiter Limiter = new Limiter(3000);
        IConnection Connection;

        public EventBusQueue(string queueUrl)
        {
            QueueUrl = queueUrl;

            Factory = new ConnectionFactory()
            {
                HostName = Host,
                UserName = UserName,
                Password = Password,
                Port = Port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            };

            if (EnableSSL)
            {
                ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                Factory.Ssl = new SslOption
                {
                    Enabled = true,
                    Version = System.Security.Authentication.SslProtocols.Tls12 // or Tls13 if supported
                };
            }

            Connection = Task.Factory.RunSync(() => Factory.CreateConnectionAsync());
            Client = Task.Factory.RunSync(() => Connection.CreateChannelAsync());
        }

        /// <summary>
        ///     Gets and sets the property MaxNumberOfMessages.
        ///     The maximum number of messages to return. Amazon SQS never returns more messages
        ///     than this value (however, fewer messages might be returned). Valid values: 1
        ///     to 10. Default: 1.
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = Config.Get("RabbitMQ:EventBusQueue:MaxNumberOfMessages", 10);
        public string Host { get; set; } = Config.Get("RabbitMQ:Host");
        public string UserName { get; set; } = Config.Get("RabbitMQ:Username");
        public string Password { get; set; } = Config.Get("RabbitMQ:Password");
        public int Port { get; set; } = Config.Get("RabbitMQ:Port", 5672);
        public bool EnableSSL { get; set; } = Config.Get("RabbitMQ:EnableSSL", false);

        /// <summary>
        ///     Gets and sets the property VisibilityTimeout.
        ///     The duration (in seconds) that the received messages are hidden from subsequent
        ///     retrieve requests after being retrieved by a ReceiveMessage request.
        /// </summary>
        public int VisibilityTimeout { get; set; } = Config.Get("RabbitMQ:EventBusQueue:VisibilityTimeout", 300);

        async Task EnsureQueueBindings()
        {
            await Client.QueueDeclareAsync(queue: QueueUrl, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await Client.ExchangeDeclareAsync(exchange: QueueUrl, type: ExchangeType.Fanout, durable: true);
            await Client.QueueBindAsync(queue: QueueUrl, exchange: QueueUrl, routingKey: QueueUrl);
        }

        public async Task<string> Publish(string message)
        {
            await Limiter.Add(1);
            await EnsureQueueBindings();

            var body = Encoding.UTF8.GetBytes(message);
            var properties = new BasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            await Client.BasicPublishAsync(exchange: "", routingKey: QueueUrl, mandatory: true, basicProperties: properties, body: body);

            return "";
        }

        public Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages) => PublishBatch(messages, 0);

        public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages, int retry = 0)
        {
            int outstandingMessageCount = 0;
            await Limiter.Add(messages.Count());
            await EnsureQueueBindings();

            foreach (var message in messages)
            {
                var body = Encoding.UTF8.GetBytes(message);
                var properties = new BasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                await Client.BasicPublishAsync(exchange: "", routingKey: QueueUrl, mandatory: true, basicProperties: properties, body: body);

                if (++outstandingMessageCount == BATCH_SIZE)
                {
                    // https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet
                    //Client.WaitForConfirmsOrDie(5.Seconds());
                    outstandingMessageCount = 0;
                }
            }

            //if (outstandingMessageCount > 0)
                // https://www.rabbitmq.com/tutorials/tutorial-seven-dotnet
                //Client.WaitForConfirmsOrDie(5.Seconds());


            //var request = Client.CreateBasicPublishBatch();

            //messages.Do(message =>
            //    request.Add(QueueUrl, QueueUrl, false, null, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message))));

            ////if (IsFifo)
            ////{
            ////    request.Entries
            ////        .ForEach(message =>
            ////    {
            ////        message.MessageDeduplicationId =
            ////            JsonConvert.DeserializeObject<JObject>(message.MessageBody)["DeduplicationId"]?.ToString();

            ////        message.MessageGroupId = "Default";
            ////    });
            ////}

            //await Limiter.Add(messages.Count());

            //request.Publish();

            ////var successfuls = response.Successful.Select(m => m.MessageId).ToList();

            ////if (response.Failed.Any())
            ////{
            ////    if (retry > MAX_RETRY)
            ////        throw new Exception("Failed to send all requests because : " + response.Failed.Select(f => f.Code).ToString(Environment.NewLine));

            ////    Log.For(this)
            ////        .Warning($"Failed to send {response.Failed.Select(c => c.Message).ToLinesString()} because : {response.Failed.Select(c => c.Code).ToLinesString()}. Retrying for {retry}/{MAX_RETRY}.");

            ////    var toSend = response.Failed.Select(f => f.Message);
            ////    successfuls.AddRange(await PublishBatch(toSend, retry++));
            ////}

            ////return successfuls;


            return new List<string>();
        }

        public void Subscribe(Func<string, Task> handler) => new Subscriber(this, handler).Start();

        public async Task PullAll(Func<string, Task> handler)
        {
            while (true)
            {
                // Instead of having multiple 10-secs waits it's better to have multiple 1-sec waits
                // Also this change will make it more Olive.Mvc.PerformanceMonitoringMiddleware friendly
                var batch = await PullBatch(timeoutSeconds: 1);
                if (batch.None()) return;

                foreach (var item in batch)
                {
                    await handler(item.RawMessage);
                    await item.Complete();
                }
            }
        }

        public async Task<IEnumerable<QueueMessageHandle>> PullBatch(int timeoutSeconds = 10,
            int? maxNumerOfMessages = null)
        {
            await Task.Run(() => { }); // placeholder to keep async context
            var result = new List<QueueMessageHandle>();

            var consumer = new AsyncEventingBasicConsumer(Client);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Log.For(this).Info($"RabbitMQ received message: Queue {QueueUrl}");

                result.Add(new QueueMessageHandle(
                    message,
                    ea.DeliveryTag.ToString(),
                    () => Task.Run(() => Client.BasicAckAsync(ea.DeliveryTag, false))
                ));
            };

            await Client.BasicConsumeAsync(
                 queue: QueueUrl,
                 autoAck: false,
                 consumer: consumer
             );

            // NOTE: RabbitMQ does not "batch" by default — this simulates a short poll delay
            await Task.Delay(timeoutSeconds * 1000);

            return result;
        }

        public Task<QueueMessageHandle> Pull(int timeoutSeconds = 10) =>
            PullBatch(timeoutSeconds, 1).FirstOrDefault();

        public Task Purge() => Client.QueuePurgeAsync(QueueUrl);
    }
}
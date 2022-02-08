using Newtonsoft.Json;
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
        internal string QueueUrl;
        public IModel Client { get; set; }
        internal bool IsFifo => QueueUrl.EndsWith(".fifo");
        readonly Limiter Limiter = new Limiter(3000);

        public EventBusQueue(string queueUrl)
        {
            QueueUrl = queueUrl;

            var factory = new ConnectionFactory() { HostName = Host, UserName = UserName, Password = Password };
            factory.Port = Port;

            if (EnableSSL)
            {
                ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
                factory.Ssl = new SslOption { Enabled = true, Version = System.Security.Authentication.SslProtocols.Tls11 | System.Security.Authentication.SslProtocols.Tls12 };

            }

            var connection = factory.CreateConnection();
            Client = connection.CreateModel();
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

        public async Task<string> Publish(string message)
        {
            await Limiter.Add(1);

            //var request = new SendMessageRequest
            //{
            //    QueueUrl = QueueUrl,
            //    MessageBody = message,
            //};

            //if (IsFifo)
            //{
            //    request.MessageDeduplicationId = JsonConvert.DeserializeObject<JObject>(message)["DeduplicationId"]?.ToString();
            //    request.MessageGroupId = "Default";
            //}
            var body = Encoding.UTF8.GetBytes(message);
            Client.QueueDeclare(QueueUrl, true, false, false, null);
            Client.ExchangeDeclare(exchange: QueueUrl, type: ExchangeType.Direct);
            Client.BasicPublish(exchange: QueueUrl,
                                 routingKey: QueueUrl,
                                 basicProperties: null,
                                 body: body);
            return "";
        }

        public Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages) => PublishBatch(messages, 0);

        public async Task<IEnumerable<string>> PublishBatch(IEnumerable<string> messages, int retry = 0)
        {
            var request = Client.CreateBasicPublishBatch();

            messages.Do(message =>
                request.Add(QueueUrl, QueueUrl, false, null, new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message))));

            //if (IsFifo)
            //{
            //    request.Entries
            //        .ForEach(message =>
            //    {
            //        message.MessageDeduplicationId =
            //            JsonConvert.DeserializeObject<JObject>(message.MessageBody)["DeduplicationId"]?.ToString();

            //        message.MessageGroupId = "Default";
            //    });
            //}

            await Limiter.Add(messages.Count());

            request.Publish();

            //var successfuls = response.Successful.Select(m => m.MessageId).ToList();

            //if (response.Failed.Any())
            //{
            //    if (retry > MAX_RETRY)
            //        throw new Exception("Failed to send all requests because : " + response.Failed.Select(f => f.Code).ToString(Environment.NewLine));

            //    Log.For(this)
            //        .Warning($"Failed to send {response.Failed.Select(c => c.Message).ToLinesString()} because : {response.Failed.Select(c => c.Code).ToLinesString()}. Retrying for {retry}/{MAX_RETRY}.");

            //    var toSend = response.Failed.Select(f => f.Message);
            //    successfuls.AddRange(await PublishBatch(toSend, retry++));
            //}

            //return successfuls;

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

        public async Task<IEnumerable<QueueMessageHandle>> PullBatch(int timeoutSeconds = 10, int? maxNumerOfMessages = null)
        {
            await Task.Run(() => { });
            var result = new List<QueueMessageHandle>();

            var consumer = new EventingBasicConsumer(Client);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Log.For(this)
                    .Info($"RabbitMQ recieved message: Queue " + QueueUrl);
                result.Add(new QueueMessageHandle(message, ea.DeliveryTag.ToString(), () => Task.Run(() => Client.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false))));
            };

            Client.BasicConsume(queue: QueueUrl,
                                autoAck: false,
                                consumer: consumer);

            return result;
        }

        public Task<QueueMessageHandle> Pull(int timeoutSeconds = 10) => PullBatch(timeoutSeconds, 1).FirstOrDefault();

        public Task Purge() => Task.Run(() => Client.QueuePurge(QueueUrl));
    }
}
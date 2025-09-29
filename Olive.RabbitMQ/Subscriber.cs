using Newtonsoft.Json.Schema;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Olive.RabbitMQ
{
    class Subscriber
    {
        public Func<string, Task> Handler { get; }
        EventBusQueue Queue;
        Thread PollingThread;

        public Subscriber(EventBusQueue queue, Func<string, Task> handler)
        {
            Handler = handler;
            Queue = queue;
        }

        public void Start()
        {
            (PollingThread = new Thread(async () => await KeepPolling())).Start();
        }

        public Task PullAll() => KeepPolling(PullStrategy.UntilEmpty, waitTimeSeconds: 10);

        private async Task DeclareQueueExchange()
        {
            await Queue.Client.ExchangeDeclareAsync(exchange: Queue.QueueUrl, type: ExchangeType.Fanout, durable: true);
            await Queue.Client.QueueDeclareAsync(Queue.QueueUrl, true, false, false, null);
            await Queue.Client.QueueBindAsync(queue: Queue.QueueUrl,
                  exchange: Queue.QueueUrl,
                  routingKey: Queue.QueueUrl);
        }

        async Task<BasicGetResult> Fetch()
        {
            try
            {
                //lock (Queue.Client)
                {
                    return await Queue.Client.BasicGetAsync(queue: Queue.QueueUrl,
                                autoAck: false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch from Queue " + Queue.QueueUrl, ex);
            }
        }


        async Task Poll()
        {
            var consumer = new AsyncEventingBasicConsumer(Queue.Client);
            consumer.ReceivedAsync += async (model, ea) =>
               {
                   try
                   {
                       var body = ea.Body.ToArray();
                       var message = Encoding.UTF8.GetString(body);
                       Log.For(this)
                           .Info($"RabbitMQ recieved message: Queue " + Queue.QueueUrl);
                       await Handler(message);
                       await Queue.Client.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                   }
                   catch (Exception ex)
                   {
                       var exception = new Exception("Failed to run queue event handler " +
                           Handler.Method.DeclaringType.GetProgrammingName() + "." +
                           Handler.Method.Name +
                           "message: " + ea.DeliveryTag.ToString()?.ToJsonText(), ex);

                           Log.For<Subscriber>().Error(exception);
                   }
               };

            //lock (Queue.Client)
            //{
                await Queue.Client.ExchangeDeclareAsync(exchange: Queue.QueueUrl, type: ExchangeType.Fanout, durable: true);
                await Queue.Client.QueueDeclareAsync(Queue.QueueUrl, true, false, false, null);
                await Queue.Client.QueueBindAsync(queue: Queue.QueueUrl,
                      exchange: Queue.QueueUrl,
                      routingKey: Queue.QueueUrl);
                await Queue.Client.BasicQosAsync(0, 1, false);
                await Queue.Client.BasicConsumeAsync(queue: Queue.QueueUrl,
                                    autoAck: false,
                                    consumer: consumer);
            //}

            Log.For<Subscriber>().Info(Queue.QueueUrl);

        }

        async Task<bool> Poll(int waitTimeSeconds)
        {
            var result = await Fetch();

            if (result == null)
            {
                await Task.Delay(waitTimeSeconds.Seconds());
                return false;
            }

            try
            {
                var body = result.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await Handler(message);
                //lock (Queue.Client)
                {
                   await Queue.Client.BasicAckAsync(deliveryTag: result.DeliveryTag, multiple: false);
                }
            }
            catch (Exception ex)
            {
                var exception = new Exception("Failed to run queue event handler " +
                    Handler.Method.DeclaringType.GetProgrammingName() + "." +
                    Handler.Method.Name +
                    "message: " + result.DeliveryTag.ToString()?.ToJsonText(), ex);

                if (Queue.IsFifo)
                    throw exception;
                else
                    Log.For<Subscriber>().Error(exception);
            }

            return true;


        }


        async Task KeepPolling(PullStrategy strategy = PullStrategy.KeepPulling, int waitTimeSeconds = 10)
        {
            await Poll();
            //DeclareQueueExchange();

            //var queueIsEmpty = false;
            //do
            //{
            //    try
            //    {
            //        queueIsEmpty = !await Poll(consumer);
            //    }
            //    catch (Exception exception) 
            //    { 
            //        Log.For<Subscriber>().Error(exception);
            //    }

            //}
            //while (strategy == PullStrategy.KeepPulling || !queueIsEmpty);
        }
    }
}
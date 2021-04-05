using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;



class AzureMessagingContext : IAsyncDisposable
{
    string QueueName;
    AzureMessagingContext()
    {

    }

    public AzureMessagingContext(string queueName)
    {
        QueueName = queueName;
    }

    ServiceBusSender _Sender;
    internal ServiceBusSender Sender => _Sender ??= Client.CreateSender(QueueName);
    ServiceBusReceiver _Receiver;
    internal ServiceBusReceiver Receiver => _Receiver ??= Client.CreateReceiver(QueueName);
    internal ServiceBusReceiver _Purger;
    internal ServiceBusReceiver Purger => _Purger ??= Client.CreateReceiver(QueueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
    readonly ServiceBusClient Client;

    public ValueTask DisposeAsync()
    {
        return Client.DisposeAsync();
    }
}

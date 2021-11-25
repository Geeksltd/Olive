using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System;
using System.Linq;
using System.Threading.Tasks;
using Olive;

class AzureMessagingContext : IAsyncDisposable
{
    string QueueUrl;
    string QueueName => QueueUrl?.Split('/').Last();
    string ServiceBusFullyQualifiedName => QueueUrl?.TrimStart("https://").Split('/').First();
    AzureMessagingContext()
    {
    }

    public AzureMessagingContext(string queueUrl)
    {
        QueueUrl = queueUrl;
        Client = new ServiceBusClient(ServiceBusFullyQualifiedName, new DefaultAzureCredential());
    }

    ServiceBusSender _Sender;
    internal ServiceBusSender Sender => _Sender ??= Client.CreateSender(QueueName);
    ServiceBusReceiver _Receiver;
    internal ServiceBusReceiver Receiver => _Receiver ??= Client.CreateReceiver(QueueName);
    internal ServiceBusReceiver _Purger;
    internal ServiceBusReceiver Purger => _Purger ??= Client.CreateReceiver(QueueName, new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });
    readonly ServiceBusClient Client;

    public ValueTask DisposeAsync() => Client.DisposeAsync();
}
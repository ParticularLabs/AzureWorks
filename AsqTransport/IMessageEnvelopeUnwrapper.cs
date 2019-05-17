namespace NServiceBus.Transport.AzureStorageQueues
{
    using Azure.Transports.WindowsAzureStorageQueues;
    using Microsoft.WindowsAzure.Storage.Queue;

    public interface IMessageEnvelopeUnwrapper
    {
        MessageWrapper Unwrap(CloudQueueMessage rawMessage);
    }
}
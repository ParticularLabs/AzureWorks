namespace FunctionApp
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System.Threading.Tasks;
    using NServiceBus.AzureFuntions;
    using Microsoft.WindowsAzure.Storage.Queue;

    //TODO
    public static class AsqConnectedFunction
    {
        static AsqConnectedFunction()
        {
            endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(SomeRoutedMessage), endpointName); //route to our self just to demo
        }

        [FunctionName(endpointName)] //this is the "one function to all many handler for different messages"
        public static Task Run([QueueTrigger(endpointName, Connection = "AzureWebJobsStorage")]CloudQueueMessage message,
            [Queue("some-queue", Connection = "AzureWebJobsStorage")]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context)
        {
            return endpoint.Invoke(message, logger, collector, context);
        }

        static FunctionsAwareStorageQueueEndpoint endpoint;

        const string endpointName = "sales";
    }
}
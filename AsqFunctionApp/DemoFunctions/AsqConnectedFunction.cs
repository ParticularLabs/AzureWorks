namespace FunctionApp
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System.Threading.Tasks;
    using NServiceBus.AzureFuntions;
    using Microsoft.WindowsAzure.Storage.Queue;

    public static class AsqConnectedFunction
    {
        static AsqConnectedFunction()
        {
            endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(SomeRoutedMessage), endpointName); // route to our self just to demo
        }

        [FunctionName(endpointName)] // this is the "one function to all handlers for different messages" - A junction function
        public static Task Run([QueueTrigger(endpointName, Connection = FunctionsConstants.ConnectionString)]CloudQueueMessage message,
            [Queue("some-queue", Connection = FunctionsConstants.ConnectionString)]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context)
        {
            return endpoint.Invoke(message, logger, collector, context);
        }

        static readonly FunctionsAwareStorageQueueEndpoint endpoint;

        const string endpointName = "sales-asq";
    }
}
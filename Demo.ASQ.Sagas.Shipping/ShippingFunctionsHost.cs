namespace Demo.ASQ.Sagas.Shipping
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System.Threading.Tasks;
    using NServiceBus.AzureFuntions;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Demo.ASQ.Sagas.Messages;

    public static class ShippingFunctionsHost
    {
        static ShippingFunctionsHost()
        {
            endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);

            // done manually
      //      endpoint.Routing.RegisterPublisher(typeof(OrderBooked), "booking");
      //      endpoint.Routing.RegisterPublisher(typeof(OrderBilled), "billing");

            endpoint.UseNServiceBusPoisonMessageHandling("error");
        }

        [FunctionName(endpointName)] // this function acts like a message pump
        public static Task Run([QueueTrigger(endpointName, Connection = FunctionsConstants.ConnectionString)]CloudQueueMessage message,
            ILogger logger,
            ExecutionContext context)
        {
            return endpoint.Invoke(message, logger, null, context);
        }

        static readonly FunctionsAwareStorageQueueEndpoint endpoint;

        const string endpointName = "shipping";
    }
}
using System;

namespace Demo.ASQ.Sagas.Sales
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System.Threading.Tasks;
    using NServiceBus.AzureFuntions;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Demo.ASQ.Sagas.Messages;

    public static class SalesFunctionsHost
    {
        static FunctionsAwareStorageQueueEndpoint Initialize()
        {
            var endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(PlaceOrder), endpointName);
            endpoint.Routing.RouteToEndpoint(typeof(BookOrder), "booking");
            endpoint.Routing.RouteToEndpoint(typeof(BillOrder), "billing");

            endpoint.UseNServiceBusPoisonMessageHandling("error");

            return endpoint;
        }

        [FunctionName(endpointName)] // this function acts like a message pump
        public static Task Run([QueueTrigger(endpointName, Connection = FunctionsConstants.ConnectionString)]CloudQueueMessage message,
            ILogger logger,
            ExecutionContext context)
        {
            return Endpoint.Value.Invoke(message, logger, null, context);
        }

        public static readonly Lazy<FunctionsAwareStorageQueueEndpoint> Endpoint = new Lazy<FunctionsAwareStorageQueueEndpoint>(Initialize);

        const string endpointName = "sales";
    }
}
namespace Demo.ASQ.Sagas.Billing
{
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System.Threading.Tasks;
    using NServiceBus.AzureFuntions;
    using Microsoft.WindowsAzure.Storage.Queue;

    public static class BillingFunctionsHost
    {
        static BillingFunctionsHost()
        {
            endpoint = new FunctionsAwareStorageQueueEndpoint(endpointName);
           
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

        const string endpointName = "billing";
    }
}
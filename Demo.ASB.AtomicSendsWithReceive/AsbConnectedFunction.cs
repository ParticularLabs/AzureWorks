namespace Demo.ASB
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;

    public static class AsbConnectedFunction
    {
        static AsbConnectedFunction()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(SomeRoutedMessage), "sales-atomic-2"); //route to our self just to demo

            //use NSB for poison message handling to not have failed messages go into the DLQ
            endpoint.UseNServiceBusPoisonMessageHandling("error");

            endpoint.UseNServiceBusAuditQueue("audit");
        }

        [FunctionName(endpointName)] // this is the "one function to all handlers for different messages" - A junction function
        public static Task Run([ServiceBusTrigger(endpointName, Connection = NServiceBus.FunctionsConstants.ConnectionString)]Message message,
            [ServiceBus("some-queue", Connection = NServiceBus.FunctionsConstants.ConnectionString)]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context,
            MessageReceiver messageReceiver)
        {
            return endpoint.Invoke(message, logger, collector, context, messageReceiver);
        }

        static readonly FunctionsAwareServiceBusEndpoint endpoint;

        const string endpointName = "sales-atomic";
    }
}
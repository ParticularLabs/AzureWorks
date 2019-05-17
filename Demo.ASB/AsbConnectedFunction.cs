namespace Demo.ASB
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;

    public static class AsbConnectedFunction
    {
        static AsbConnectedFunction()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName);

            endpoint.Routing.RouteToEndpoint(typeof(SomeRoutedMessage), endpointName); //route to our self just to demo
        }

        [FunctionName(endpointName)] // this is the "one function to all handlers for different messages" - A junction function
        public static Task Run([ServiceBusTrigger(endpointName, Connection = NServiceBus.FunctionsConstants.ConnectionString)]Message message,
            [ServiceBus("some-queue", Connection = NServiceBus.FunctionsConstants.ConnectionString)]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context)
        {
            return endpoint.Invoke(message, logger, collector, context);
        }

        static readonly FunctionsAwareServiceBusEndpoint endpoint;

        const string endpointName = "sales";
    }
}
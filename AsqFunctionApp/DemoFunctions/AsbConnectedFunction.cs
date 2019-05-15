namespace FunctionApp
{
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using NServiceBus;
    using System.Threading.Tasks;

    [System.ComponentModel.EditorBrowsableAttribute]

    public static class AsbConnectedFunction
    {
        static AsbConnectedFunction()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName, connectionStringName);

            endpoint.Routing.RouteToEndpoint(typeof(SomeRoutedMessage), endpointName); //route to our self just to demo
        }

        [FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        public static Task Run([ServiceBusTrigger(endpointName, Connection = connectionStringName)]Message message,
            [ServiceBus("some-queue", Connection = "my-sb-connstring")]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context)
        {
            return endpoint.Invoke(message, logger, collector, context);
        }

        static FunctionsAwareServiceBusEndpoint endpoint;

        const string endpointName = "sales";
        const string connectionStringName = "my-sb-connstring";
    }
}
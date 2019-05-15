using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Logging;

namespace AsbFunctionApp
{
    using System.Threading.Tasks;


    //TODO: create a collector transport?
    public static class AsbSalesTrigger
    {
        static AsbSalesTrigger()
        {
            endpoint = new FunctionsAwareServiceBusEndpoint(endpointName, connectionStringName);
        }

        //TODO: what if this was using a HttpTrigger
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
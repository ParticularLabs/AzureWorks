using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace AsqFunctionApp
{
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Queue;

    public static class Sales
    {

        //TODO: pros and cons with using a static CTOR needs to be investigated
        static Sales()
        {
            var ec = new EndpointConfiguration(endpointName);

            ec.AssemblyScanner().OnlyTypesFrom(typeof(Sales).Assembly);
            ec.UseTransport<AzureFunctionsASB>()
                .ConnectionString(SomeReader.Read(sbConnString));
            ec.UseAzureFunctionDelayedLogger();//figure this out
            endpoint = Endpoint.Start(ec).GetAwaiter().GetResult();
        }

        [FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        public static async Task Run([QueueTrigger(endpointName, Connection = sbConnString)]CloudQueueMessage message, ILogger log, IAsyncCollector<string> outputStuff)
        {
            return endpoint.Invoke("", message, log, outputStuff);
        }

        private IEndpointInstance endpoint;

        private const string sbConnString = "sb://my-namespace";

        private const string endpointName = "sales-process-order";


    }

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            await context.AddToCollector("some-payload"); //push stuff out via native connectors

            return context.Publish(new OrderPlaced());//emit messages to the ASB namespace we received the message from
        }
    }

    internal class OrderPlaced : IEvent
    {
    }

    internal class PlaceOrder : ICommand

    {
    }
}


//{
//"name": "nsbEmitter",
//"type": "NServiceBus",
//"connection": "MyServiceBusConnection",
//"direction": "out"
//}
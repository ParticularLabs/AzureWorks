using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Transport;

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

          
            ec.UseTransport<AzureServiceBusTransport>()
                .ConnectionString("todo");
            //ec.UseAzureFunctionDelayedLogger();//figure this out
            var instance = Endpoint.Start(ec).GetAwaiter().GetResult();

            endpoint = new FunctionsAwareEndpoint(instance);
        }

        [FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        public static Task Run([ServiceBusTrigger(endpointName, Connection = sbConnString)]CloudQueueMessage message, ILogger log, IAsyncCollector<string> collector)
        {
            //todo: what if this was using a HttpTrigger
            return endpoint.Invoke(message, log, collector);
        }

        private static FunctionsAwareEndpoint endpoint;

        private const string sbConnString = "sb://my-namespace";

        private const string endpointName = "sales-process-order";


    }

    class FunctionsAwareEndpoint
    {
        private readonly IEndpointInstance endpointInstance;

        public FunctionsAwareEndpoint(IEndpointInstance endpointInstance)
        {
            this.endpointInstance = endpointInstance;
        }

        public Task Invoke<T>(CloudQueueMessage message, ILogger log, IAsyncCollector<T> collector)
        {
            throw new NotImplementedException();
            //TODO: marshal the logger
            ////TODO: get the collector into the root context
            //var messageContext = new MessageContext(message.Id,new Dictionary<string, string>(me), );
            //return endpointInstance.PushMessage();
        }
    }

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            //await context.AddToCollector("some-payload"); //push stuff out via native connectors

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
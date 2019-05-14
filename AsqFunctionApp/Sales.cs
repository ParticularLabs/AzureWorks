using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Transport.AzureServiceBus;

namespace AsqFunctionApp
{
    using System.Threading.Tasks;

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
        public static Task Run([ServiceBusTrigger(endpointName, Connection = sbConnString)]Message message, ILogger log, IAsyncCollector<string> collector)
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

        public Task Invoke<T>(Message message, ILogger log, IAsyncCollector<T> collector)
        {
            //TODO: marshal the logger
            //TODO: get the collector into the root context

            var messageId = message.GetMessageId();
            var headers = message.GetNServiceBusHeaders();
            var body = message.GetBody();

            var rootContext = new ContextBag();

            rootContext.Set(collector);

            var messageContext = new MessageContext(messageId, headers, body, new TransportTransaction(), new CancellationTokenSource(), rootContext);
            return endpointInstance.PushMessage(messageContext);
        }
    }

    static class MessageContextExtensions
    {
        public static IAsyncCollector<T> GetAsyncCollector<T>(this IMessageHandlerContext context)
        {
            return context.Extensions.Get<IAsyncCollector<T>>();
        }
    }

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            await context.GetAsyncCollector<string>()
                .AddAsync("some-payload"); //push stuff out via native connectors

            await context.Publish(new OrderPlaced());//emit messages to the ASB namespace we received the message from
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
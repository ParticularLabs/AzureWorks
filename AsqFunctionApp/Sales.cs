using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace AsqFunctionApp
{
    using System.Threading.Tasks;

    public static class Sales
    {

        //TODO: pros and cons with using a static CTOR needs to be investigated
        static Sales()
        {
            var ec = new EndpointConfiguration(endpointName);

            ec.GetSettings().Set("hack-do-not-use-the-pump", true);
            ec.UseTransport<AzureServiceBusTransport>()
                .ConnectionString("PUT SB CONNSTRING HERE");
            
            var instance = Endpoint.Start(ec).GetAwaiter().GetResult();

            endpoint = new FunctionsAwareEndpoint(instance);
        }

        [FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        public static Task Run([ServiceBusTrigger(endpointName, Connection = "my-sb-connstring")]Message message, ILogger log, [ServiceBus("some-queue", Connection = "my-sb-connstring")]IAsyncCollector<string> collector)
        {
            //todo: what if this was using a HttpTrigger

            return endpoint.Invoke(message, log, collector);
        }

        private static FunctionsAwareEndpoint endpoint;

        private const string endpointName = "sales";
    }

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            Console.Out.WriteLine("Place order!");
            
            await context.GetAsyncCollector<string>()
                .AddAsync("some-payload"); //push stuff out via native connectors

            await context.Publish(new OrderPlaced());//emit messages to the ASB namespace we received the message from
            await context.SendLocal(new SomeLocalMessage());
        }
    }

    class SomeLocalMessageHandler : IHandleMessages<SomeLocalMessage>
    {
        public Task Handle(SomeLocalMessage message, IMessageHandlerContext context)
        {
            Console.Out.WriteLine("Got the local message");

            return Task.CompletedTask;
        }
    }
}

internal class OrderPlaced : IEvent
{
}

internal class PlaceOrder : ICommand

{
}


internal class SomeLocalMessage : IMessage
{
}
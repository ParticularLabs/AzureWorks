using System;
using System.Diagnostics;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace AsbFunctionApp
{
    using System.Threading.Tasks;

    public static class Sales
    {
        static Sales()
        {
            //TODO: create a collector transport?
            endpoint = new FunctionsAwareEndpoint(endpointName);
        }


        //TODO: what if this was using a HttpTrigger
        [FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        public static Task Run([ServiceBusTrigger(endpointName, Connection = "my-sb-connstring")]Message message,
            [ServiceBus("some-queue", Connection = "my-sb-connstring")]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context)
        {
            return endpoint.Invoke(message, logger, collector, context);
        }

        static FunctionsAwareEndpoint endpoint;
        const string endpointName = "sales";
    }

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            context.GetLogger().LogInformation("Place order!");

            //TODO: Do we force users to always use a string? If yes we can skip the generic on this method
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
            context.GetLogger().LogInformation("Got the local message");

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
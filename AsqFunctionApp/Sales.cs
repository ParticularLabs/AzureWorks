using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace AsqFunctionApp
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public static class Sales
    {

        //TODO: pros and cons with using a static CTOR needs to be investigated
        static Sales()
        {
            var ec = new EndpointConfiguration(endpointName);

            ec.GetSettings().Set("hack-do-not-use-the-pump", true);
            ec.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString"));
            
            var instance = Endpoint.Start(ec).GetAwaiter().GetResult();

            endpoint = new FunctionsAwareEndpoint(instance);
        }

        [FunctionName(endpointName)]//this is the "one function to all many handler for different messages"
        public static Task Run([ServiceBusTrigger(endpointName, Connection = "my-sb-connstring")]Message message, 
            [ServiceBus("some-queue", Connection = "my-sb-connstring")]IAsyncCollector<string> collector,
            ILogger logger,
            ExecutionContext context)
        {
            //TODO: what if this was using a HttpTrigger

            #region how to get access to the connection string using ExecutionContext

            // should be assigned to a static not to create all the time

            var configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
            logger.LogInformation(configuration["my-sb-connstring"]);

            #endregion

            return endpoint.Invoke(message, logger, collector);
        }

        private static FunctionsAwareEndpoint endpoint;

        private const string endpointName = "sales";
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
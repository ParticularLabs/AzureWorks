using System.Threading.Tasks;
using NServiceBus;

namespace AsbFunctionApp
{
    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            //TODO: Use NSB logger
            //context.GetLogger().LogInformation("Place order!");

            //TODO: Do we force users to always use a string? If yes we can skip the generic on this method
            await context.GetAsyncCollector<string>()
                .AddAsync("some-payload"); //push stuff out via native connectors

            await context.Publish(new OrderPlaced());//emit messages to the ASB namespace we received the message from
            await context.SendLocal(new SomeLocalMessage());
        }

    }
}
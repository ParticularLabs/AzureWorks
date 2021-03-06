using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace FunctionApp
{
    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.Info("Place order!");

            await context.GetAsyncCollector<string>()
                .AddAsync("some-payload"); //push stuff out via native connectors

            await context.Publish(new OrderPlaced());//emit messages to the ASB namespace we received the message from
            await context.SendLocal(new SomeLocalMessage());
            await context.SendLocal(new SomeRoutedMessage());
        }

        static ILog logger = LogManager.GetLogger<PlaceOrderHandler>();
    }
}
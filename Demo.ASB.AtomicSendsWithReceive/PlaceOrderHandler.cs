namespace Demo.ASB
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.Info("Handler with real SendsAtomicWithReceive transaction mode!");

            await context.Send(new SomeRoutedMessage()); // sending to "sales-atomic-2" endpoint/queue in an atomic fashion
        }

        static ILog logger = LogManager.GetLogger<PlaceOrderHandler>();
    }
}
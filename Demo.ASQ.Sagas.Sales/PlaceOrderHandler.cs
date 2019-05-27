namespace Demo.ASQ.Sagas.Sales
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Demo.ASQ.Sagas.Messages;

    class PlaceOrderHandler : IHandleMessages<PlaceOrder>
    {
        public async Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            logger.Info("Placing order!");

            await context.Send(new BookOrder() { OrderId = message.OrderId });
            await context.Send(new BillOrder() { OrderId = message.OrderId });
        }

        static ILog logger = LogManager.GetLogger<PlaceOrderHandler>();
    }
}
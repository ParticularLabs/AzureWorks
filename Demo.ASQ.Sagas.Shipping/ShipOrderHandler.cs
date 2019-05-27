namespace Demo.ASQ.Sagas.Sales
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Demo.ASQ.Sagas.Messages;

    class ShipOrderHandler : IHandleMessages<ShipOrder>
    {
        public async Task Handle(ShipOrder message, IMessageHandlerContext context)
        {
            logger.Info("shipping order!");

            await context.Publish(new OrderShipped
            {
                OrderId = message.OrderId
            });
        }

        static ILog logger = LogManager.GetLogger<ShipOrderHandler>();
    }
}
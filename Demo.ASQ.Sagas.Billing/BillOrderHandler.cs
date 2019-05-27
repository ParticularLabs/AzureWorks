namespace Demo.ASQ.Sagas.Billing
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Demo.ASQ.Sagas.Messages;

    class BillOrderHandler : IHandleMessages<BillOrder>
    {
        public async Task Handle(BillOrder message, IMessageHandlerContext context)
        {
            logger.Info("Billing order!");

            await context.Publish(new OrderBilled()
            {
                OrderId = message.OrderId
            });
        }

        static ILog logger = LogManager.GetLogger<BillOrderHandler>();
    }
}
namespace Demo.ASQ.Sagas.Booking
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;
    using Demo.ASQ.Sagas.Messages;

    class BookOrderHandler : IHandleMessages<BookOrder>
    {
        public async Task Handle(BookOrder message, IMessageHandlerContext context)
        {
            logger.Info("Booking order!");

            await context.Publish(new OrderBooked()
            {
                OrderId = message.OrderId
            });
        }

        static ILog logger = LogManager.GetLogger<BookOrderHandler>();
    }
}
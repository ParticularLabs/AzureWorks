using System.Threading.Tasks;
using Demo.ASQ.Sagas.Messages;
using NServiceBus;
using NServiceBus.Logging;

namespace Demo.ASQ.Sagas.Shipping
{
    public class ShippingPolicy : Saga<ShippingPolicyData>,
        IAmStartedByMessages<OrderBooked>,
        IAmStartedByMessages<OrderBilled>
    {
        static ILog log = LogManager.GetLogger<ShippingPolicy>();

        public async Task Handle(OrderBooked message, IMessageHandlerContext context)
        {
            log.Info($"Received OrderBooked, OrderId = {message.OrderId}");
            Data.OrderBooked = true;
            await ShipOrder(context);
        }

        public async Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            log.Info($"Received OrderBilled, OrderId = {message.OrderId}");
            Data.OrderBilled = true;
            await ShipOrder(context);
        }

        private async Task ShipOrder(IMessageHandlerContext context)
        {
            if (Data.OrderBooked && Data.OrderBilled)
            {
                await context.SendLocal(new ShipOrder() { OrderId = Data.OrderId });
                MarkAsComplete();
            }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
        {
            mapper.ConfigureMapping<OrderBooked>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
            mapper.ConfigureMapping<OrderBilled>(message => message.OrderId)
                .ToSaga(sagaData => sagaData.OrderId);
        }
    }
}
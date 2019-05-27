namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class OrderShipped : IEvent
    {
        public string OrderId { get; set; }
    }
}
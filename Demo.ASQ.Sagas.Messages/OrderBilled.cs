namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class OrderBilled : IEvent
    {
        public string OrderId { get; set; }
    }
}
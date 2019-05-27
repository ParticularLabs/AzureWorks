namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class OrderBooked : IEvent
    {
        public string OrderId { get; set; }
    }
}
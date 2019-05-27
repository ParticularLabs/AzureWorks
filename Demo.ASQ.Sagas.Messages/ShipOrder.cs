namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class ShipOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}
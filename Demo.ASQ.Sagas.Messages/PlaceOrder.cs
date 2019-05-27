namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class PlaceOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}
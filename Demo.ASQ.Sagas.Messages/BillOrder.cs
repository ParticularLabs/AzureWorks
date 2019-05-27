namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class BillOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}

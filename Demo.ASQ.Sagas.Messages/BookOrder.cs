namespace Demo.ASQ.Sagas.Messages
{
    using NServiceBus;

    public class BookOrder : ICommand
    {
        public string OrderId { get; set; }
    }
}


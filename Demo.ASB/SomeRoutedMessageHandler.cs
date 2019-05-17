namespace Demo.ASB
{
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Logging;

    class SomeRoutedMessageHandler : IHandleMessages<SomeRoutedMessage>
    {
        public Task Handle(SomeRoutedMessage message, IMessageHandlerContext context)
        {
            logger.Info("Got the routed message");

            return Task.CompletedTask;
        }

        static ILog logger = LogManager.GetLogger<SomeLocalMessageHandler>();
    }
}
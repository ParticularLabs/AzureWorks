using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class SomeMessageHandler : IHandleMessages<SomeMessage>
{
    public Task Handle(SomeMessage message, IMessageHandlerContext context)
    {
        logger.Info("SomeMessage called");
        return Task.CompletedTask;
    }

    static ILog logger = LogManager.GetLogger<SomeMethodHandler>();
}
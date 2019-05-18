using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class SomeOtherMethodHandler : IHandleMessages<SomeOtherMethod>
{
    public Task Handle(SomeOtherMethod message, IMessageHandlerContext context)
    {
        logger.Info("SomeOtherMethod called");
        return Task.CompletedTask;
    }

    static ILog logger = LogManager.GetLogger<SomeMethodHandler>();
}
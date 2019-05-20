using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

class SomeMethodHandler:IHandleMessages<SomeMethod>
{
    public Task Handle(SomeMethod message, IMessageHandlerContext context)
    {
        logger.Info("SomeMethod called");

       // throw new Exception("Simulated http");

        return context.SendLocal(new SomeMessage());
    }

    static ILog logger = LogManager.GetLogger<SomeMethodHandler>();
}
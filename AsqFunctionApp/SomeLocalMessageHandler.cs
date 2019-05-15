using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

namespace AsbFunctionApp
{
    class SomeLocalMessageHandler : IHandleMessages<SomeLocalMessage>
    {
        public Task Handle(SomeLocalMessage message, IMessageHandlerContext context)
        {
            logger.Info("Got the local message");
           
            return Task.CompletedTask;
        }

        static ILog logger = LogManager.GetLogger<SomeLocalMessageHandler>();
    }
}
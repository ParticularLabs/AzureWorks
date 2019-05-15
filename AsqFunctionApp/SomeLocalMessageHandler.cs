using System;
using System.Threading.Tasks;
using NServiceBus;

namespace AsbFunctionApp
{
    class SomeLocalMessageHandler : IHandleMessages<SomeLocalMessage>
    {
        public Task Handle(SomeLocalMessage message, IMessageHandlerContext context)
        {
            //context.GetLogger().LogInformation("Got the local message");

            Console.Out.WriteLine("SomeLocalMessage");

            return Task.CompletedTask;
        }
    }
}
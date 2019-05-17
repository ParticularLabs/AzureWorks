namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Logging;
    using Pipeline;
    using Unicast;

    class WarnAgainstMultipleHandlersForSameMessageTypeBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        readonly MessageHandlerRegistry messageHandlerRegistry;

        public WarnAgainstMultipleHandlersForSameMessageTypeBehavior(MessageHandlerRegistry messageHandlerRegistry)
        {
            this.messageHandlerRegistry = messageHandlerRegistry;
       }

        public override Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            var type = context.Message.MessageType;
            var numberOfHandlers = messageHandlerRegistry.GetHandlersFor(type).Count;
            
            if (numberOfHandlers > 1)
            {
                logger.WarnFormat("Message of type '{0}' is handled by more than a single handler ({1}). Ensure transport supports transactions or capable of retries to have more than one handler. Otherwise break down into multiple message types each handled by a separate handler."
                    , type, numberOfHandlers);
            }

            return next();
        }

        static ILog logger = LogManager.GetLogger<WarnAgainstMultipleHandlersForSameMessageTypeBehavior>();
    }
}
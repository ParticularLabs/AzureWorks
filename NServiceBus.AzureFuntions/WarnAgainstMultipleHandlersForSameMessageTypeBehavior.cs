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
                logger.WarnFormat("Message of type {0} is handled by more that a single handler ({1}). Ensure transport supports transactions or retried to have more than one handler or break down into multiple message types passed from one handler to another."
                    , type, numberOfHandlers);
            }

            return next();
        }

        static ILog logger = LogManager.GetLogger<WarnAgainstMultipleHandlersForSameMessageTypeBehavior>();
    }
}
namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Unicast.Messages;

    public class PassThroughBehavior : Behavior<IIncomingPhysicalMessageContext>
    {
        private readonly MessageMetadataRegistry messageMetadataRegistry;
        private readonly Func<string, string> passThroughRoutingRule;

        public PassThroughBehavior(MessageMetadataRegistry messageMetadataRegistry, Func<string,string> passThroughRoutingRule)
        {
            this.messageMetadataRegistry = messageMetadataRegistry;
            this.passThroughRoutingRule = passThroughRoutingRule;
        }

        public override Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            var messageType = context.MessageHeaders[Headers.EnclosedMessageTypes];
            var messageMetadata = messageMetadataRegistry.GetMessageMetadata(messageType);

            if (messageMetadata == null)
            {
                var destination = passThroughRoutingRule(messageType);
                return context.ForwardCurrentMessageTo(destination);
            }

            return next();
        }
    }
}
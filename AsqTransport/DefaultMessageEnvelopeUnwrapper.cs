using System;

namespace NServiceBus.Transport.AzureStorageQueues
{
    using System.IO;
    using System.Runtime.Serialization;
    using Azure.Transports.WindowsAzureStorageQueues;
    using Microsoft.WindowsAzure.Storage.Queue;

    class DefaultMessageEnvelopeUnwrapper : IMessageEnvelopeUnwrapper
    {
        public DefaultMessageEnvelopeUnwrapper(MessageWrapperSerializer messageSerializer)
        {
            messageWrapperSerializer = messageSerializer;
        }

        public MessageWrapper Unwrap(CloudQueueMessage rawMessage)
        {
            MessageWrapper m;
            using (var stream = new MemoryStream(rawMessage.AsBytes))
            {
                try
                {
                    m = messageWrapperSerializer.Deserialize(stream);
                }
                catch (Exception e)
                {
                    //not a NSB envelope
                    return new MessageWrapper
                    {
                        Body = rawMessage.AsBytes
                    };
                }
            }

            if (m == null)
            {
                throw new SerializationException("Message is null");
            }

            //TODO: We should raise a dev ex issue for this since we should consider this change since it helps native integrations
            if (m.Headers == null)
            {
                //not a NSB envelope
                return new MessageWrapper
                {
                    Body = rawMessage.AsBytes
                };
            }

            if (m.ReplyToAddress != null)
            {
                m.Headers[Headers.ReplyToAddress] = m.ReplyToAddress;
            }
            m.Headers[Headers.CorrelationId] = m.CorrelationId;

            m.Headers[Headers.MessageIntent] = m.MessageIntent.ToString(); // message intent extension method

            return m;
        }

        MessageWrapperSerializer messageWrapperSerializer;
    }
}
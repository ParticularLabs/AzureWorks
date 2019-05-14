using NServiceBus.Transport;

namespace NServiceBus
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an endpoint in the running phase.
    /// </summary>
    public interface IEndpointInstance : IMessageSession
    {
        /// <summary>
        /// Stops the endpoint.
        /// </summary>
        Task Stop();

        /// <summary>
        /// Manually pushes a message into the endpoint
        /// </summary>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        Task PushMessage(MessageContext messageContext);
    }
}
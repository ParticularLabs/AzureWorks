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


        /// <summary>
        /// Manually pushes a message into recoverability
        /// </summary>
        /// <param name="errorContext"></param>
        /// <returns></returns>
        Task<ErrorHandleResult> PushError(ErrorContext errorContext);

        /// <summary>
        /// Transport transaction mode
        /// </summary>
        TransportTransactionMode TransportTransactionMode { get; }
    }
}
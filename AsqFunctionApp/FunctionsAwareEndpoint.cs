using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Transport.AzureServiceBus;

namespace AsqFunctionApp
{
    class FunctionsAwareEndpoint
    {
        private readonly IEndpointInstance endpointInstance;

        public FunctionsAwareEndpoint(IEndpointInstance endpointInstance)
        {
            this.endpointInstance = endpointInstance;
        }

        public Task Invoke(Message message, ILogger logger, IAsyncCollector<string> collector)
        {
            var messageId = message.GetMessageId();
            var headers = message.GetNServiceBusHeaders();
            var body = message.GetBody();

            var rootContext = new ContextBag();
            rootContext.Set(collector);
            rootContext.Set(logger);

            var messageContext = new MessageContext(messageId, headers, body, new TransportTransaction(), new CancellationTokenSource(), rootContext);

            return endpointInstance.PushMessage(messageContext);
        }
    }
}
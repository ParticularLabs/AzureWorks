using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Transport.AzureServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace NServiceBus
{
    class FunctionsAwareServiceBusEndpoint
    {
        public FunctionsAwareServiceBusEndpoint(string endpointName, string serviceBusConnectionStringName)
        {
            this.endpointName = endpointName;
            this.serviceBusConnectionStringName = serviceBusConnectionStringName;
        }
        public async Task Invoke(Message message, ILogger logger, IAsyncCollector<string> collector, ExecutionContext executionContext)
        {
            var messageId = message.GetMessageId();
            var headers = message.GetNServiceBusHeaders();
            var body = message.GetBody();

            var rootContext = new ContextBag();
            rootContext.Set(collector);
            rootContext.Set(logger);

            var messageContext = new MessageContext(messageId, headers, body, new TransportTransaction(), new CancellationTokenSource(), rootContext);

            var instance = await GetEndpoint(logger, executionContext);

            //TODO: error handling
            await instance.PushMessage(messageContext);
        }

        async Task<IEndpointInstance> GetEndpoint(ILogger logger, ExecutionContext executionContext)
        {

            //TODO: locking or lazy
            if (endpointInstance != null)
            {
                return endpointInstance;
            }

            endpointInstance = await InitializeEndpoint(logger, executionContext);

            return endpointInstance;
        }

        Task<IEndpointInstance> InitializeEndpoint(ILogger logger, ExecutionContext executionContext)
        {
            var ec = new EndpointConfiguration(endpointName);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(executionContext.FunctionDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            ec.GetSettings().Set("hack-do-not-use-the-pump", true);
            ec.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(configuration[serviceBusConnectionStringName]);

            return Endpoint.Start(ec);
        }

        IEndpointInstance endpointInstance;

        readonly string endpointName;
        readonly string serviceBusConnectionStringName;
    }
}
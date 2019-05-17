using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Configuration.AdvancedExtensibility;
using Microsoft.WindowsAzure.Storage.Queue;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;
using NServiceBus.Transport.AzureStorageQueues;

namespace NServiceBus.AzureFuntions
{
    using System;
    using Unicast.Messages;

    public class FunctionsAwareStorageQueueEndpoint
    {
        public FunctionsAwareStorageQueueEndpoint(string endpointName)
        {
            endpointConfiguration = new EndpointConfiguration(endpointName);

            endpointConfiguration.GetSettings().Set("hack-do-not-use-the-pump", true);
            endpointConfiguration.UseSerialization<XmlSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            transport = endpointConfiguration.UseTransport<AzureStorageQueueTransport>();
            transport.DelayedDelivery().DisableTimeoutManager();

            Routing = transport.Routing();
        }

        public async Task Invoke(CloudQueueMessage message, ILogger logger, IAsyncCollector<string> collector, ExecutionContext executionContext)
        {
            var instance = await GetEndpoint(logger, executionContext);

            var messageId = message.Id;           

            var unwrapped = unwrapper.Unwrap(message);

            var headers = unwrapped.Headers;
            var body = unwrapped.Body;

            var rootContext = new ContextBag();
            rootContext.Set(collector);

            var messageContext = new MessageContext(messageId, headers, body, new TransportTransaction(), new CancellationTokenSource(), rootContext);
            
            //TODO: right now the native retries are used, should we have an option to move to "our" error?
            await instance.PushMessage(messageContext);
        }

        public void EnablePassThroughRoutingForUnknownMessages(Func<string, string> routingRule)
        {
            endpointConfiguration.Pipeline.Register(b =>
            {
                var registry = endpointConfiguration.GetSettings().Get<MessageMetadataRegistry>();

                return new PassThroughBehavior(registry, routingRule);
            }, "Forwards unknown messages to the configured destination");
        }

        public async Task Send<T>(T message, ILogger logger, ExecutionContext executionContext)
        {
            var instance = await GetEndpoint(logger, executionContext);

            await instance.Send(message);
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
            NServiceBus.Logging.LogManager.UseFactory(new MsExtLoggerFactory(logger));


            var configuration = new ConfigurationBuilder()
                .SetBasePath(executionContext.FunctionDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            transport.ConnectionString(configuration[FunctionsConstants.ConnectionString]);

            var instance = Endpoint.Start(endpointConfiguration);

            unwrapper = EnvelopeWrapperBuilder.BuildUnwrapper(endpointConfiguration.GetSettings());

            return instance;

        }

        public RoutingSettings Routing { get; }

        EndpointConfiguration endpointConfiguration;
        IEndpointInstance endpointInstance;
        TransportExtensions<AzureStorageQueueTransport> transport;
        IMessageEnvelopeUnwrapper unwrapper;
    }
}

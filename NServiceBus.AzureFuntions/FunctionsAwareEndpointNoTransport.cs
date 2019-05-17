using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Configuration.AdvancedExtensibility;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace NServiceBus
{
    public class FunctionsAwareEndpointNoTransport
    {
        public FunctionsAwareEndpointNoTransport(string endpointName)
        {
            endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.GetSettings().Set("hack-do-not-use-the-pump", true);

            transport = endpointConfiguration.UseTransport<NoopTransport>();
            transport.ConnectionString("no-op");

            Routing = transport.Routing();
        }

        public async Task Invoke(HttpRequest request, ILogger logger, IAsyncCollector<string> collector, ExecutionContext executionContext)
        {
            var messageId = Guid.NewGuid().ToString("N");
            var headers = new Dictionary<string, string>();
            var memoryStream = new MemoryStream();

            await request.Body.CopyToAsync(memoryStream);

            var body = memoryStream.ToArray(); //JsonConvert.DeserializeObject(memoryStream.ToArray(), typeof(PlaceOrder)); // TODO: hardcoded, needs to be determined

            var rootContext = new ContextBag();
            if (collector == null)
            {
                collector = new FakeCollector<string>();
            }
            rootContext.Set(collector);

            var messageContext = new MessageContext(messageId, headers, body, new TransportTransaction(), new CancellationTokenSource(), rootContext);

            var instance = await GetEndpoint(logger, executionContext);

            await instance.PushMessage(messageContext);
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

            return Endpoint.Start(endpointConfiguration);
        }

        public RoutingSettings Routing { get; }

        EndpointConfiguration endpointConfiguration;
        IEndpointInstance endpointInstance;
        TransportExtensions<NoopTransport> transport;
    }

    class FakeCollector<T> : IAsyncCollector<T>
    {
        public Task AddAsync(T item, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }
}
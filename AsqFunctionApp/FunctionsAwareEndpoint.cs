using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Transport.AzureServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace NServiceBus
{
    class FunctionsAwareEndpoint
    {
        private readonly string endpointName;
        private readonly IEndpointInstance endpointInstance;

        public FunctionsAwareEndpoint(string endpointName)
        {
            this.endpointName = endpointName;
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

            var instance = await GetEndpoint(logger, collector, executionContext);

            //TODO: error handling
            await instance.PushMessage(messageContext);
        }

        Task<IEndpointInstance> GetEndpoint(ILogger logger, IAsyncCollector<string> collector, ExecutionContext executionContext)
        {
            //todo: proper lazy or locking
            if (endpointInstance != null)
            {
                return Task.FromResult(endpointInstance);
            }

            return InitializeEndpoint(logger, collector, executionContext);
        }

        Task<IEndpointInstance> InitializeEndpoint(ILogger logger, IAsyncCollector<string> collector, ExecutionContext context)
        {
            var ec = new EndpointConfiguration(endpointName);

            #region how to get access to the connection string using ExecutionContext

            // should be assigned to a static not to create all the time

            //var configuration = new ConfigurationBuilder()
            //    .SetBasePath(executionContext.FunctionDirectory)
            //    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
            //    .AddEnvironmentVariables()
            //    .Build();
            //logger.LogInformation(configuration["my-sb-connstring"]);

            #endregion

            ec.GetSettings().Set("hack-do-not-use-the-pump", true);
            ec.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(Environment.GetEnvironmentVariable("AzureServiceBus_ConnectionString"));

            return Endpoint.Start(ec);
        }
    }
}
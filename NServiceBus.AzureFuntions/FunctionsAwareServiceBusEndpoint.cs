using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Transport.AzureServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.Unicast.Messages;
using NServiceBus.Unicast;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;


namespace NServiceBus
{
    using System.Transactions;
    using Microsoft.Azure.ServiceBus.Core;

    public class FunctionsAwareServiceBusEndpoint
    {
        public FunctionsAwareServiceBusEndpoint(string endpointName)
        {
            endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.GetSettings().Set("hack-do-not-use-the-pump", true);

            transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();

            Routing = transport.Routing();

            WarnAgainstMultipleHandlersForSameMessageType();
        }

        public async Task Invoke(Message message, ILogger logger, IAsyncCollector<string> collector, ExecutionContext executionContext, MessageReceiver messageReceiver = null)
        {
            var messageId = message.GetMessageId();
            var headers = message.GetNServiceBusHeaders();
            var body = message.GetBody();

            if (!headers.ContainsKey(Headers.ConversationId))
            {
                headers[Headers.ConversationId] = messageId;
            }


            var rootContext = new ContextBag();
            if (collector == null)
            {
                collector = new FakeCollector<string>();
            }
            rootContext.Set(collector);

            var instance = await GetEndpoint(logger, executionContext);
            
            try
            {
                // TODO: only should be done if in sends atomic with receive mode and message receiver is provided
                var useTransaction = messageReceiver != null;

                using (var scope =  useTransaction ? new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled) : null)
                {
                    var transportTransaction = CreateTransportTransaction(useTransaction, messageReceiver, message.PartitionKey);
                    var messageContext = new MessageContext(messageId, headers, body, transportTransaction, new CancellationTokenSource(), rootContext);

                    await instance.PushMessage(messageContext);

                    // Azure Function auto-completion would be disabled if we try to run in SendsAtomicWithReceive, need to complete message manually
                    if (useTransaction)
                    {
                        await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
                    }

                    scope?.Complete();
                }

            }
            catch (Exception ex)
            {
                // TODO: is 4 the right value?
                // TODO: Should we provide delayed retries as well?
                // TODO: when using transaction, complete the incoming message along with sending the error message
                if (moveFailedMessagesToError && message.SystemProperties.DeliveryCount > 4)
                {
                    var errorContext = new ErrorContext(ex, headers, messageId, body, new TransportTransaction(), 0);

                    var result = await instance.PushError(errorContext);

                    if (result == ErrorHandleResult.RetryRequired)
                    {
                        throw;
                    }

                    return;
                }

                throw;
            }
        }

        TransportTransaction CreateTransportTransaction(bool useTransaction, MessageReceiver messageReceiver, string incomingQueuePartitionKey)
        {
            var transportTransaction = new TransportTransaction();

            if (useTransaction)
            {
                transportTransaction.Set((messageReceiver.ServiceBusConnection, messageReceiver.Path));
                transportTransaction.Set("IncomingQueue.PartitionKey", incomingQueuePartitionKey);
            }

            return transportTransaction;
        }


        public async Task<IActionResult> Invoke(HttpRequest request, string messageType, ILogger logger, IAsyncCollector<string> collector, ExecutionContext executionContext)
        {
            var messageId = Guid.NewGuid().ToString("N");
            var headers = new Dictionary<string, string> { [Headers.EnclosedMessageTypes] = messageType };

            foreach (var httpHeader in request.Headers)
            {
                headers[httpHeader.Key] = httpHeader.Value;
            }

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

            try
            {
                await instance.PushMessage(messageContext);
            }
            catch (Exception ex)
            {
                if (moveFailedMessagesToError)
                {
                    var errorContext = new ErrorContext(ex, headers, messageId, body, new TransportTransaction(), 0);

                    var result = await instance.PushError(errorContext);

                    if (result == ErrorHandleResult.RetryRequired)
                    {
                        throw;
                    }

                    return new AcceptedResult();
                }

                throw;
            }

            return new OkResult();
        }

        public async Task Send<T>(T message, ILogger logger, ExecutionContext executionContext)
        {
            var instance = await GetEndpoint(logger, executionContext);

            await instance.Send(message);
        }

        public void UseNServiceBusPoisonMessageHandling(string errorQueue)
        {
            endpointConfiguration.Recoverability().CustomPolicy((c, e) =>
            {
                return RecoverabilityAction.MoveToError(errorQueue);
            });

            moveFailedMessagesToError = true;
        }

        public void UseNServiceBusAuditQueue(string auditQueue)
        {
            endpointConfiguration.AuditProcessedMessagesTo(auditQueue);
        }

        public void EnablePassThroughRoutingForUnknownMessages(Func<string, string> routingRule)
        {
            endpointConfiguration.Pipeline.Register(b =>
            {
                var registry = endpointConfiguration.GetSettings().Get<MessageMetadataRegistry>();

                return new PassThroughBehavior(registry, routingRule);
            }, "Forwards unknown messages to the configured destination");
        }

        void WarnAgainstMultipleHandlersForSameMessageType()
        {
            endpointConfiguration.Pipeline.Register(builder => new WarnAgainstMultipleHandlersForSameMessageTypeBehavior(builder.Build<MessageHandlerRegistry>()), "Warns against multiple handlers for same message type");
        }

        async Task<IEndpointInstance> GetEndpoint(ILogger logger, ExecutionContext executionContext)
        {
            semaphoreLock.Wait();

            if (endpointInstance != null)
            {
                return endpointInstance;
            }

            endpointInstance = await InitializeEndpoint(logger, executionContext);

            semaphoreLock.Release();

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


            return Endpoint.Start(endpointConfiguration);
        }

        public RoutingSettings Routing { get; }

        EndpointConfiguration endpointConfiguration;
        IEndpointInstance endpointInstance;
        TransportExtensions<AzureServiceBusTransport> transport;
        bool moveFailedMessagesToError;
        SemaphoreSlim semaphoreLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
    }
}
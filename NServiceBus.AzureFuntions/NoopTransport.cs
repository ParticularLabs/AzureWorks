namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DelayedDelivery;
    using Extensibility;
    using Performance.TimeToBeReceived;
    using Routing;
    using Settings;
    using Transport;

    public class NoopTransport : TransportDefinition
    {
        public override string ExampleConnectionStringForErrorMessage { get; } = string.Empty;

        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            return new NoopTransportInfrastructure();
        }
    }

    class NoopTransportInfrastructure : TransportInfrastructure
    {
        public override IEnumerable<Type> DeliveryConstraints
        {
            get
            {
                yield return typeof(DelayDeliveryWith);
                yield return typeof(NonDurableDelivery);
                yield return typeof(DoNotDeliverBefore);
                yield return typeof(DelayedDeliveryConstraint);
                yield return typeof(DiscardIfNotReceivedBefore);
            }
        }

        public override TransportTransactionMode TransactionMode { get; } = TransportTransactionMode.None;

        public override OutboundRoutingPolicy OutboundRoutingPolicy { get; } = new OutboundRoutingPolicy(OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast);

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure()
        {
            return new TransportReceiveInfrastructure(
                messagePumpFactory:() => new NoOpPump(),
                queueCreatorFactory:() => new NoOpQueueCreator(),
                preStartupCheck: () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure()
        {
            return new TransportSendInfrastructure(
                dispatcherFactory: () => new NoOpMessageDispatcher(),
                preStartupCheck: () => Task.FromResult(StartupCheckResult.Success));
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure()
        {
            return new TransportSubscriptionInfrastructure(
                subscriptionManagerFactory: () => new NoOpSubscriptionManager());
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            return new EndpointInstance(instance.Endpoint);
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return logicalAddress.EndpointInstance.Endpoint;
        }
    }

    class NoOpSubscriptionManager : IManageSubscriptions
    {
        public Task Subscribe(Type eventType, ContextBag context)
        {
            return Task.CompletedTask;
        }

        public Task Unsubscribe(Type eventType, ContextBag context)
        {
            return Task.CompletedTask; ;
        }
    }

    class NoOpMessageDispatcher : IDispatchMessages
    {
        public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, ContextBag context)
        {
            return Task.CompletedTask;
        }
    }

    class NoOpQueueCreator : ICreateQueues
    {
        public Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            return Task.CompletedTask;
        }
    }

    class NoOpPump : IPushMessages
    {
        public Task Init(Func<MessageContext, Task> onMessage, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings settings)
        {
            return Task.CompletedTask;
        }

        public void Start(PushRuntimeSettings limitations)
        {
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}
namespace NServiceBus
{
    using System;
    using System.Reflection;
    using MessageInterfaces;
    using Routing;
    using Serialization;
    using Settings;
    using Transport;
    using Transport.AzureStorageQueues;

    /// <summary>
    /// Transport definition for AzureStorageQueue
    /// </summary>
    public class AzureStorageQueueTransport : TransportDefinition, IMessageDrivenSubscriptionTransport
    {
        internal const string SerializerSettingsKey = "MainSerializer";

        /// <inheritdoc cref="RequiresConnectionString"/>
        public override bool RequiresConnectionString { get; } = true;

        /// <inheritdoc cref="ExampleConnectionStringForErrorMessage"/>
        public override string ExampleConnectionStringForErrorMessage { get; } =
            "DefaultEndpointsProtocol=[http|https];AccountName=myAccountName;AccountKey=myAccountKey";

        /// <inheritdoc cref="Initialize"/>
        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            Guard.AgainstNull(nameof(settings), settings);
            Guard.AgainstNullAndEmpty(nameof(connectionString), connectionString);

            Guard.AgainstUnsetSerializerSetting(settings);

            DefaultConfigurationValues.Apply(settings);

            return new AzureStorageQueueInfrastructure(settings, connectionString);
        }
    }
}
﻿namespace ServiceBus.Management.AcceptanceTests.Contexts.TransportIntegration
{
    using System;
    using NServiceBus;

    public class AzureServiceBusTransportIntegration : ITransportIntegration
    {
        public AzureServiceBusTransportIntegration()
        {
            ConnectionString = ""; // empty on purpose
        }

        public string Name { get { return "AzureServiceBus"; } }
        public Type Type { get { return typeof(AzureServiceBus); } }
        public string TypeName { get { return "NServiceBus.AzureServiceBus, NServiceBus.Azure.Transports.WindowsAzureServiceBus"; } }
        public string ConnectionString { get; set; }

        public void Cleanup(ITransportIntegration transport)
        {

        }
    }
}

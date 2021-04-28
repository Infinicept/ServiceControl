﻿namespace ServiceControl.Transports.ASB
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using NServiceBus.CustomChecks;
    using NServiceBus.Logging;

    public class CheckDeadLetterQueue : CustomCheck
    {
        public CheckDeadLetterQueue(TransportSettings settings) : base(id: "Dead Letter Queue", category: CustomChecksCategories.ServiceControlTransportHealth, repeatAfter: TimeSpan.FromHours(1))
        {
            Logger.Debug("Azure Service Bus Dead Letter Queue custom check starting");

            connectionString = settings.ConnectionString;
            stagingQueue = $"{settings.EndpointName}.staging";
            runCheck = settings.RunCustomChecks;
        }

        public override Task<CheckResult> PerformCheck()
        {
            if (namespaceManager == null)
            {
                namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            }

            if (!runCheck)
            {
                return CheckResult.Pass;
            }

            Logger.Debug("Checking Dead Letter Queue length");

            var queueDescription = namespaceManager.GetQueue(stagingQueue);
            var messageCountDetails = queueDescription.MessageCountDetails;

            if (messageCountDetails.DeadLetterMessageCount > 0)
            {
                var result = $"{messageCountDetails.DeadLetterMessageCount} messages in the Dead Letter Queue '{stagingQueue}'. This could indicate a problem with ServiceControl's retries. Please submit a support ticket to Particular using support@particular.net if you would like help from our engineers to ensure no message loss while resolving these dead letter messages.";

                Logger.Warn(result);
                return CheckResult.Failed(result);
            }

            Logger.Debug("No messages in Dead Letter Queue");
            return CheckResult.Pass;
        }

        NamespaceManager namespaceManager;
        string stagingQueue;
        bool runCheck;
        string connectionString;

        static readonly ILog Logger = LogManager.GetLogger(typeof(CheckDeadLetterQueue));
    }
}
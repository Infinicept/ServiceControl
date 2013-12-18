﻿namespace ServiceControl.MessageFailures.Handlers
{
    using Contracts.MessageFailures;
    using NServiceBus;
    using ServiceBus.Management.Infrastructure.RavenDB;

    public class MessageFailureResolvedHandler : IHandleMessages<MessageFailureResolvedByRetry>
    {
        public RavenUnitOfWork RavenUnitOfWork { get; set; }

        public void Handle(MessageFailureResolvedByRetry message)
        {
            var failedMessage = RavenUnitOfWork.Session.Load<FailedMessage>(message.FailedMessageId);

            if (failedMessage == null)
            {
                return; //No point throwing
            }

            failedMessage.Status = FailedMessageStatus.Resolved;    
        }
    }
}
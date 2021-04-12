﻿namespace ServiceControl.AcceptanceTests.Recoverability.ExternalIntegration
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Infrastructure;
    using NServiceBus;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.Settings;
    using ServiceBus.Management.Infrastructure.Settings;
    using NUnit.Framework;
    using ServiceControl.Contracts;
    using ServiceControl.MessageFailures;
    using TestSupport;
    using TestSupport.EndpointTemplates;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    class When_a_failed_message_is_unarchived : AcceptanceTest
    {
        [Test]
        public async Task Should_publish_notification()
        {
            CustomConfiguration = config => config.OnEndpointSubscribed<MyContext>((s, ctx) =>
            {
                if (s.SubscriberReturnAddress.IndexOf("ExternalProcessor", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ctx.ExternalProcessorSubscribed = true;
                }
            });

            var context = await Define<MyContext>()
                .WithEndpoint<Receiver>(b => b.When(c => c.ExternalProcessorSubscribed, async bus =>
                {
                    await bus.SendLocal<MyMessage>(m => m.MessageNumber = 1)
                        .ConfigureAwait(false);
                }).DoNotFailOnErrorMessages())
                .WithEndpoint<ExternalProcessor>(b => b.When(async (bus, c) =>
                {
                    await bus.Subscribe<FailedMessagesUnArchived>();

                    if (c.HasNativePubSubSupport)
                    {
                        c.ExternalProcessorSubscribed = true;
                    }
                }))
                .Do("WaitUntilErrorsContainsFaildMessage", async ctx =>
                {
                    if (ctx.FailedMessageId == null)
                    {
                        return false;
                    }

                    return await this.TryGet<FailedMessage>($"/api/errors/{ctx.FailedMessageId}",
                        e => e.Status == FailedMessageStatus.Unresolved);
                })
                .Do("Archive", async ctx =>
                {
                    await this.Post<object>($"/api/errors/{ctx.FailedMessageId}/archive");
                })
                .Do("EnsureMessageIsArchived", async ctx =>
                {
                    return await this.TryGet<FailedMessage>($"/api/errors/{ctx.FailedMessageId}",
                        e => e.Status == FailedMessageStatus.Archived);
                })
                .Do("UnArchive", async ctx =>
                {
                    await this.Patch<object>($"/api/errors/unarchive", new List<string> { ctx.FailedMessageId });
                })
                .Do("EnsureUnArchived", async ctx =>
                {
                    return await this.TryGet<FailedMessage>($"/api/errors/{ctx.FailedMessageId}",
                        e => e.Status == FailedMessageStatus.Unresolved);
                })
                .Done(ctx => ctx.EventDelivered) //Done when sequence is finished
                .Run();

            var deserializedEvent = JsonConvert.DeserializeObject<FailedMessagesUnArchived>(context.Event);
            Assert.IsTrue(Array.IndexOf(deserializedEvent.FailedMessagesIds, context.FailedMessageId) > -1);
            Assert.IsNotNull(deserializedEvent.FailedMessagesIds);
            Assert.IsNotNull(deserializedEvent.MessagesCount);
        }

        public class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    c.NoDelayedRetries();
                    c.ReportSuccessfulRetriesToServiceControl();
                });
            }

            public class MyMessageHandler : IHandleMessages<MyMessage>
            {
                public MyContext Context { get; set; }
                public ReadOnlySettings Settings { get; set; }

                public Task Handle(MyMessage message, IMessageHandlerContext context)
                {
                    var messageId = context.MessageId.Replace(@"\", "-");

                    var uniqueMessageId = DeterministicGuid.MakeId(messageId, Settings.EndpointName()).ToString();

                    if (message.MessageNumber == 1)
                    {
                        Context.FailedMessageId = uniqueMessageId;
                    }

                    if (Context.FailProcessing)
                    {
                        throw new Exception("Simulated exception");
                    }

                    return Task.FromResult(0);
                }
            }
        }

        public class ExternalProcessor : EndpointConfigurationBuilder
        {
            public ExternalProcessor()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var routing = c.ConfigureTransport().Routing();
                    routing.RouteToEndpoint(typeof(FailedMessagesUnArchived).Assembly, Settings.DEFAULT_SERVICE_NAME);
                }, publisherMetadata => { publisherMetadata.RegisterPublisherFor<FailedMessagesUnArchived>(Settings.DEFAULT_SERVICE_NAME); });
            }

            public class FailureHandler : IHandleMessages<FailedMessagesUnArchived>
            {
                public MyContext Context { get; set; }

                public Task Handle(FailedMessagesUnArchived message, IMessageHandlerContext context)
                {
                    var serializedMessage = JsonConvert.SerializeObject(message);
                    Context.Event = serializedMessage;
                    Context.EventDelivered = true;
                    return Task.FromResult(0);
                }
            }
        }

        public class MyMessage : ICommand
        {
            public int MessageNumber { get; set; }
        }

        public class MyContext : ScenarioContext, ISequenceContext
        {
            public string FailedMessageId { get; set; }
            public string GroupId { get; set; }
            public bool FailProcessing { get; set; } = true;
            public int Step { get; set; }
            public bool ExternalProcessorSubscribed { get; set; }
            public string Event { get; set; }
            public bool EventDelivered { get; set; }
        }
    }
}
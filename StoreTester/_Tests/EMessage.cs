using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStoreIntegrationTester.Domain.Counter;
using EventStoreIntegrationTester.Domain.Counter.Messages;
using EventStoreIntegrationTester.Domain.EMessage.Messages;
using EventStoreIntegrationTester._Tests;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;
using FluentAssertions;

namespace EventStoreIntegrationTester._Tests
{
    //[Only]
    public class EMessageCreation: BaseTest
    {
        public EMessageCreation(IDomainCommandExecutor commandExecutor, IEventStore eventStore):base(commandExecutor, eventStore, "EMessage creation"){}
        public override bool Run()
        {
            var batch = new List<ICommand>()
            {
                new CreateEMessage(Const.EMessageId,  Const.UserId1),
                new ReviseEMessageBody(Const.EMessageId, "message body"),
                new ReviseEMessageSubject(Const.EMessageId, "message subject"),
                new ReviseEMessageToRecepientList(Const.EMessageId, new List<Guid>{ Const.UserId2, Const.UserId3 }),
            };

            var result = CommandExecutor.ExecuteCommandBatch(batch, Origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);


            result = CommandExecutor.ExecuteCommand(new ReviseEMessageBody(Const.EMessageId, "new " + DateTime.Now), Origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);

            //DEMO: result = CommandExecutor.ExecuteCommand(new ReviseEMessageToRecepientList(Const.EMessageId, new List<Guid> { Const.UserId3 }), Origin);

            return !result.CommandHasFailed;
        }
    }
}

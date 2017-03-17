using System;
using System.Collections.Generic;
using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.IntegrationTests.Domain.EMessage.Messages;
using FluentAssertions;

namespace FeiEventStore.IntegrationTests._Tests
{
    //[Only]
    public class EMessageCreation: BaseTest
    {
        public EMessageCreation(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository) :base(commandExecutor, eventStore, stateRepository, "EMessage creation"){}
        public override bool Run()
        {
            var batch = new List<ICommand>()
            {
                new CreateEMessage(Const.EMessageId,  Const.UserId1),
                new ReviseEMessageBody(Const.EMessageId, "message body"),
                new ReviseEMessageSubject(Const.EMessageId, "message subject"),
                new ReviseEMessageToRecepientList(Const.EMessageId, new List<Guid>{ Const.UserId2 }),
            };

            var result = CommandExecutor.ExecuteCommandBatch(batch, Origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);


            result = CommandExecutor.ExecuteCommand(new ReviseEMessageBody(Const.EMessageId, "new " + DateTime.Now), Origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);

            result = CommandExecutor.ExecuteCommand(new ReviseEMessageToRecepientList(Const.EMessageId, new List<Guid> { Const.UserId2, Const.UserId3 }), Origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);

            result = CommandExecutor.ExecuteCommand(new SendEMessage(Const.EMessageId), Origin);


            return !result.CommandHasFailed;
        }
    }
}

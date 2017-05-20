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
        public EMessageCreation(IDomainCommandExecutor commandExecutor, IEventStore eventStore, IAggregateStateRepository stateRepository) 
            : base(commandExecutor, eventStore, stateRepository, "EMessage creation"){}
        public override bool Run()
        {
            var origin = Const.UserId1;

            var batch = new List<ICommand>()
            {
                new CreateEMessage(Const.EMessageId,  Const.UserId1),
                new ReviseEMessageBody(Const.EMessageId, "message body"),
                new ReviseEMessageSubject(Const.EMessageId, "message subject"),
                new ReviseEMessageToRecepientList(Const.EMessageId, new List<string>{ Const.UserId2 }),
            };

            var result = CommandExecutor.ExecuteCommandBatch(batch, origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);


            result = CommandExecutor.ExecuteCommand(new ReviseEMessageBody(Const.EMessageId, "new " + DateTime.Now), origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);

            result = CommandExecutor.ExecuteCommand(new ReviseEMessageToRecepientList(Const.EMessageId, new List<string> { Const.UserId2, Const.UserId3 }), origin);
            result.CommandHasFailed.ShouldBeEquivalentTo(false);

            result = CommandExecutor.ExecuteCommand(new SendEMessage(Const.EMessageId), origin);


            return !result.CommandHasFailed;
        }
    }
}

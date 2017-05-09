using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.AggregateStateRepository;
using FeiEventStore.Core;
using NLog;

namespace FeiEventStore.Domain
{
    internal class DomainExecutionScopeService : IDomainExecutionScopeService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal DomainExecutionScopeContext Context { get; set; }
        internal void Init(DomainExecutionScopeContext ctx, string originUserId)
        {
            Context = ctx;
            OriginUserId = originUserId;
        }

        internal bool CommandHasFailed { get { return Context.ExecutionResult.CommandHasFailed;  } }

        internal DomainCommandResult BuildResult(long finalStoreVesion)
        {
            Context.ExecutionResult.EventStoreVersion = finalStoreVesion;
            return Context.ExecutionResult;
        }

        public void ReportException(Exception e)
        {
            ReportFatalError(e.Message);
            Context.ExecutionResult.ExceptionStackTrace = e.StackTrace;
        }

        public void ReportFatalError(string errorMessage)
        {
            Context.ExecutionResult.Errors.Add(errorMessage);
            Context.ExecutionResult.CommandHasFailed = true;

        }

        public void ReportError(string errorMessage)
        {
            Context.ExecutionResult.Errors.Add(errorMessage);
        }

        public void ReportWarning(string warningMessage)
        {
            Context.ExecutionResult.Warnings.Add(warningMessage);
        }

        public void ReportInfo(string infoMessage)
        {
            Context.ExecutionResult.Infos.Add(infoMessage);
        }

        /// <summary>
        /// Gets the immutable state of the aggregate.
        /// This method can be used by Command validators and Process Managers.
        /// NOTE: Aggregates should not use it as this will violate DDD principles.
        /// </summary>
        /// <typeparam name="TAggregateState"></typeparam>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public TAggregateState GetImmutableAggregateState<TAggregateState>(Guid aggregateId) where TAggregateState : class, IAggregateState
        {
            var state = Context.CreateAndTrackAggregateStateClone(aggregateId);
            var result = state as TAggregateState;
            if(result == null)
            {
                var e = new InvalidAggregateStateTypeException(typeof(TAggregateState), state.GetType());
                Logger.Fatal(e);
                throw e;
            }
            return result;
        }

        public string OriginUserId { get; protected set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Events;
using FeiEventStore.Persistence;

namespace FeiEventStore.AggregateStateRepository
{
    public interface IAggregateStateRepository
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <typeparam name="TAggregateState">The type of the aggregate state.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <param name="swallowNotFoundException">if set to <c>true</c> <exception cref="SnapshotNotFoundException"/> will be swallowed and null value will be returned.</param>
        /// <returns></returns>
        /// <exception cref="InvalidAggregateStateTypeException"></exception>
        /// <exception cref="AggregateNotFoundException"></exception>
        TAggregateState Get<TAggregateState>(Guid id, bool swallowNotFoundException = true) where TAggregateState : class, IState ;
    }
}

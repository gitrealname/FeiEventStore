﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;

namespace EventStoreIntegrationTester.Domain.Counter.Messages
{
    public class Increment : ICommand
    {
        public Increment(Guid aggregateId, int by, long? targetAggregateVersion = null)
        {
            TargetAggregateId = aggregateId;
            By = by;
            TargetAggregateVersion = targetAggregateVersion;
        }
        public int By { get; set; }
        public Guid TargetAggregateId { get; set; }
        public long? TargetAggregateVersion { get; set; }
    }
    
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeiEventStore.Core;
using FeiEventStore.Domain;
using FeiEventStore.Persistence.Sql.SqlDialects;
using FluentAssertions;

namespace FeiEventStore.Persistence.Sql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new SqlPersistenceEngine(new PostgreSqlDialect("Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=estest"));
            try
            {
                Console.WriteLine("---- Cold Run ----");
                for(int i = 0; i < 1; i++)
                {
                    TimeIt("ReCreateSchema", () => ReCreateSchema(engine));

                    TimeIt("PkInsert", () => PkInsert(engine));
                    TimeIt("PkUpdate", () => PkUpdate(engine));
                    TimeIt("PkDelete", () => PkDelete(engine));
                    TimeIt("PkViolation", () => PkViolation(engine));

                    TimeIt("EventsInsert", () => EventsInsert(engine));
                    TimeIt("EventsConcurrencyViolation", () => EventsConcurrencyViolation(engine)); 
                    TimeIt("EventsAggregateConcurrencyViolation", () => EventsAggregateConcurrencyViolation(engine));

                    TimeIt("SnapshotInsert", () => SnapshotInsert(engine));
                    TimeIt("SnapshotUpdate", () => SnapshotUpdate(engine));

                    TimeIt("ProcessInsert", () => ProcessInsert(engine));
                    TimeIt("ProcessUpdate", () => ProcessUpdate(engine));
                    TimeIt("ProcessDelete", () => ProcessDelete(engine));
                    TimeIt("ProcessConcurrencyViolation", () => ProcessConcurrencyViolation(engine));

                    TimeIt("EventsGetByAggregate", () => EventsGetByAggregate(engine));
                    TimeIt("EventsGetByTimeRange", () => EventsGetByTimeRange(engine));
                    TimeIt("EventsGetByStoreVersion", () => EventsGetByStoreVersion(engine));

                    TimeIt("ProcessesGetByProcessId", () => ProcessesGetByProcessId(engine));
                    TimeIt("ProcessesGetByProcessTypeAndAggregateId", () => ProcessesGetByProcessTypeAndAggregateId(engine));

                    TimeIt("SnapshotGetByAggregateId", () => SnapshotGetByAggregateId(engine));

                    TimeIt("AggregateSnapshotProcessVersion", () => AggregateSnapshotProcessVersion(engine));

                    Console.WriteLine("---- Hot Run ----");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        static void TimeIt(string name, Action action)
        {
            var sw = new Stopwatch();
            sw.Start();
            action();
            sw.Stop();
            //Console.WriteLine($"{name}: {sw.ElapsedMilliseconds}/{sw.ElapsedTicks} msc/ticks.");
            Console.WriteLine($"{name}: {sw.Elapsed.TotalMilliseconds} msc.");
        }

        static void AggregateSnapshotProcessVersion(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 90);
            EventsInsert(engine, 90);
            SnapshotInsert(engine, 90);
            ProcessInsert(engine, 90);

            var aggregateVersion = engine.GetAggregateVersion(g1);
            var snapshotVersion = engine.GetSnapshotVersion(g1);
            var processVersion = engine.GetProcessVersion(g1);

            aggregateVersion.ShouldBeEquivalentTo(1);
            snapshotVersion.ShouldBeEquivalentTo(1);
            processVersion.ShouldBeEquivalentTo(1);
        }

        static void ReCreateSchema(SqlPersistenceEngine engine)
        {
            engine.DestroyStorage();
            engine.InitializeStorage();
        }
        static void ProcessDelete(SqlPersistenceEngine engine)
        {
            ProcessInsert(engine, 20);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20);
            var processesToDelete = new HashSet<Guid>();
            processesToDelete.Add(g1);
            engine.Commit(null, null, null, processesToDelete, null);

        }
        static void ProcessUpdate(SqlPersistenceEngine engine)
        {
            ProcessInsert(engine, 10);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11);
            var processes = new List<ProcessRecord>()
            {
                new ProcessRecord()
                {
                    ProcessId = g1,
                    InvolvedAggregateId = g2,
                    ProcessVersion = 2,
                    ProcessTypeId = "process.type.1",
                    ProcessStateTypeId = "process.state.type.10",
                    State = @"{""val"":""process.state.updated""}",
                },
                new ProcessRecord()
                {
                    ProcessId = g1,
                    InvolvedAggregateId = g1,
                    ProcessVersion = 2,
                    ProcessTypeId = "process.type.1",
                    ProcessStateTypeId = null,
                    State = null,
                },
            };
            engine.Commit(null, null, processes, null, null);

        }
        static void ProcessConcurrencyViolation(SqlPersistenceEngine engine)
        {
            ProcessInsert(engine, 30);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30);
            var exception = false;
            try
            {
                ProcessInsert(engine, 30);
            }
            catch(ProcessConcurrencyViolationException ex)
            {
                //ignore, expected
                exception = true;
                ex.ProcessId.ShouldBeEquivalentTo(g1);
            }
            exception.ShouldBeEquivalentTo(true);
        }

        static void ProcessesGetByProcessTypeAndAggregateId(SqlPersistenceEngine engine)
        {
            byte id = 60;
            var procTypeId = "ProcessesGetByProcessTypeAndAggregateId";
            ProcessInsert(engine, id, procTypeId);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);
            var g = Guid.Empty;

            var list1 = engine.GetProcessRecords(procTypeId, g1, true).ToList();
            var list2 = engine.GetProcessRecords(procTypeId, g, false).ToList();

            list1.Count.ShouldBeEquivalentTo(2);
            list2.Count.ShouldBeEquivalentTo(0);

            var exception = false;
            try
            {
                engine.GetProcessRecords(procTypeId, g, true);
            }
            catch(ProcessNotFoundException ex)
            {
                //ignore, expected
                exception = true;
            }
            exception.ShouldBeEquivalentTo(true);
        }

        static void ProcessesGetByProcessId(SqlPersistenceEngine engine)
        {
            byte id = 50;
            ProcessInsert(engine, id);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);

            var list1 = engine.GetProcessRecords(g1).ToList();

            list1.Count.ShouldBeEquivalentTo(2);
        }

        static void ProcessInsert(SqlPersistenceEngine engine, byte id = 1, string processTypeId = "process.type.1")
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)(id + 1));
            var processes = new List<ProcessRecord>()
            {
                new ProcessRecord()
                {
                    ProcessId = g1,
                    InvolvedAggregateId = g1,
                    ProcessVersion = 1,
                    ProcessTypeId = processTypeId,
                    ProcessStateTypeId = "process.state.type.1",
                    State = @"{""val"":""process.state.1""}",
                },
                new ProcessRecord()
                {
                    ProcessId = g1,
                    InvolvedAggregateId = g2,
                    ProcessVersion = 1,
                    ProcessTypeId = processTypeId,
                    ProcessStateTypeId = null,
                    State = null,
                },
            };
            engine.Commit(null, null, processes, null, null);
        }

        static void SnapshotGetByAggregateId(SqlPersistenceEngine engine)
        {
            byte id = 50;
            SnapshotInsert(engine, id);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);
            var g = Guid.Empty;

            var s = engine.GetSnapshot(g1, true);
            var s1 = engine.GetSnapshot(g, false); 

            s.AggregateId.ShouldBeEquivalentTo(g1);
            if(s1 != null)
            {
                throw new InvalidOperationException("s1 should be null.");
            }

            var exception = false;
            try
            {
                engine.GetSnapshot(g, true);
            }
            catch(SnapshotNotFoundException ex)
            {
                //ignore, expected
                exception = true;
                ex.AggregateId.ShouldBeEquivalentTo(g);
            }
            exception.ShouldBeEquivalentTo(true);

        }

        static void SnapshotInsert(SqlPersistenceEngine engine, byte id = 1)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)(id + 1));
            var snapshots = new List<SnapshotRecord>()
            {
                new SnapshotRecord()
                {
                    AggregateId = g1,
                    AggregateVersion = 1,
                    AggregateTypeId = "snapshot.insert.aggregate.type.1",
                    AggregateStateTypeId = "snapshot.insert.state.type.1",
                    State = @"{""val"":""state.1""}",
                },
                new SnapshotRecord()
                {
                    AggregateId = g2,
                    AggregateVersion = 2,
                    AggregateTypeId = "snapshot.insert.aggregate.type.2",
                    AggregateStateTypeId = "snapshot.insert.state.type.2",
                    State = @"{""val"":""state.2""}",
                },
            };
            engine.Commit(null, snapshots, null, null, null);
        }

        static void SnapshotUpdate(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10);
            var snapshots = new List<SnapshotRecord>()
            {
                new SnapshotRecord()
                {
                    AggregateId = g1,
                    AggregateVersion = 300,
                    AggregateTypeId = "snapshot.update.aggregate.type.1",
                    AggregateStateTypeId = "snapshot.update.state.type.1",
                    State = @"{""val"":""state.1""}",
                },
            };
            engine.Commit(null, snapshots, null, null, null);

            snapshots = new List<SnapshotRecord>()
            {
                new SnapshotRecord()
                {
                    AggregateId = g1,
                    AggregateVersion = 301,
                    AggregateTypeId = "snapshot.insert.update.type.1",
                    AggregateStateTypeId = "snapshot.update.state.type.1",
                    State = @"{""val"":""state.1.updated""}",
                },
            };
            engine.Commit(null, snapshots, null, null, null);
        }


        static void EventsGetByAggregate(SqlPersistenceEngine engine)
        {
            byte id = 40;
            EventsInsert(engine, id);
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)(id + 1));

            var list1 = engine.GetEvents(g1, 0, null).ToList();
            var list2 = engine.GetEvents(g2, 0, null).ToList();
            var list3 = engine.GetEvents(g1, 1, 1).ToList();

            list1.Count.ShouldBeEquivalentTo(2);
            list2.Count.ShouldBeEquivalentTo(1);
            list3.Count.ShouldBeEquivalentTo(1);
        }
        static void EventsGetByTimeRange(SqlPersistenceEngine engine)
        {
            var t0 = DateTimeOffset.UtcNow;
            EventsInsert(engine, 50);
            System.Threading.Thread.Sleep(1);
            var t1 = DateTimeOffset.UtcNow;
            EventsInsert(engine, 60);
            var t2 = DateTimeOffset.UtcNow;

            var list1 = engine.GetEvents(t0, null).ToList();
            var list2 = engine.GetEvents(t1, t2).ToList();
            var list3 = engine.GetEvents(t0, t2).ToList();

            list1.Count.ShouldBeEquivalentTo(6);
            list2.Count.ShouldBeEquivalentTo(3);
            list3.Count.ShouldBeEquivalentTo(6);
        }

        static void EventsGetByStoreVersion(SqlPersistenceEngine engine)
        {
            var v0 = engine.StoreVersion + 1;
            EventsInsert(engine, 70);
            var v1 = engine.StoreVersion;
            EventsInsert(engine, 80);

            var list1 = engine.GetEvents(v0, null).ToList();
            var list2 = engine.GetEvents(v1, 3).ToList();
            var list3 = engine.GetEvents(v0, 6).ToList();

            list1.Count.ShouldBeEquivalentTo(6);
            list2.Count.ShouldBeEquivalentTo(3);
            list3.Count.ShouldBeEquivalentTo(6);
        }

        static void EventsInsert(SqlPersistenceEngine engine, byte id = 1)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, id);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)(id + 1));
            var events = new List<EventRecord>()
            {
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 1,
                    AggregateId = g1,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.insert.1",
                    EventPayloadTypeId = "payload.type.1",
                    Payload = @"{""val"":""payload.ver.1""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 2,
                    AggregateId = g1,
                    AggregateVersion = 2,
                    AggregateTypeId = "event.insert.1",
                    EventPayloadTypeId = "payload.type.2",
                    Payload = @"{""val"":""payload.ver.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 3,
                    AggregateId = g2,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.insert.1",
                    EventPayloadTypeId = "payload.type.1",
                    Payload = @"{""val"":""payload.ver.1""}", AggregateTypeUniqueKey = null, OriginUserId = "user.1", EventTimestamp = DateTimeOffset.UtcNow,
                },
            };
            var expectedVersion = engine.StoreVersion + 3;
            var ver = engine.Commit(events, null, null, null, null);
            ver.ShouldBeEquivalentTo(expectedVersion);
        }

        static void EventsConcurrencyViolation(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20);
            var events = new List<EventRecord>()
            {
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 1,
                    AggregateId = g1,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.store.version.violation",
                    EventPayloadTypeId = "payload.type.10",
                    Payload = @"{""val"":""payload.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
            };
            var expectedVersion = engine.StoreVersion + 1;
            var ver = engine.Commit(events, null, null, null, null);
            engine.StoreVersion.ShouldBeEquivalentTo(expectedVersion);

            events = new List<EventRecord>()
            {
                new EventRecord()
                {
                    StoreVersion = ver, //collision!
                    AggregateId = g2,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.insert.10",
                    EventPayloadTypeId = "payload.type.10",
                    Payload = @"{""val"":""payload.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
            };
            var exception = false;
            try
            {
                engine.Commit(events, null, null, null, null);
            }
            catch(EventStoreConcurrencyViolationException)
            {
                //ignore, is expected
                exception = true;
            }
            engine.StoreVersion.ShouldBeEquivalentTo(expectedVersion);
            exception.ShouldBeEquivalentTo(true, "EventStoreConcurrencyViolationException is excepted.");
        }
        static void EventsAggregateConcurrencyViolation(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30);
            var events = new List<EventRecord>()
            {
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 1,
                    AggregateId = g1,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.aggr.version.violation",
                    EventPayloadTypeId = "payload.type.30",
                    Payload = @"{""val"":""payload.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
            };
            var expectedVersion = engine.StoreVersion + 1;
            engine.Commit(events, null, null, null, null);
            engine.StoreVersion.ShouldBeEquivalentTo(expectedVersion);

            events = new List<EventRecord>()
            {
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 1,
                    AggregateId = g1,
                    AggregateVersion = 1, //collision
                    AggregateTypeId = "event.insert.10",
                    EventPayloadTypeId = "payload.type.10",
                    Payload = @"{""val"":""payload.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
            };
            var exception = false;
            try
            {
                engine.Commit(events, null, null, null, null);
            }
            catch(AggregateConcurrencyViolationException)
            {
                //ignore, is expected
                exception = true;
            }
            engine.StoreVersion.ShouldBeEquivalentTo(expectedVersion);
            exception.ShouldBeEquivalentTo(true, "AggregateConcurrencyViolationException is excepted.");
        }

        static void PkInsert(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
            var g3 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
            var pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g1,
                    AggregateTypeId = "pk.insert.1",
                    PrimaryKey = "key.1",
                },
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g2,
                    AggregateTypeId = "pk.insert.1",
                    PrimaryKey = "key.2",
                },
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g3,
                    AggregateTypeId = "pk.insert.2",
                    PrimaryKey = "key.1",
                },
            };

            engine.Commit(null, null, null, null, pks);
        }

        static void PkUpdate(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10);
            var pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g1,
                    AggregateTypeId = "pk.update.1",
                    PrimaryKey = "key.1",
                },
            };
            engine.Commit(null, null, null, null, pks);

            pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g1,
                    AggregateTypeId = "pk.update.1",
                    PrimaryKey = "key.updated_value",

                },
            };
            engine.Commit(null, null, null, null, pks);
        }
        static void PkDelete(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 20);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 21);
            var pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g1,
                    AggregateTypeId = "pk.delete.1",
                    PrimaryKey = "key.to.be.deleted",
                },
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g2,
                    AggregateTypeId = "pk.delete.2",
                    PrimaryKey = "key.remains",
                },
            };
            engine.Commit(null, null, null, null, pks);

            pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g1,
                    AggregateTypeId = "pk.delete.1",
                    PrimaryKey = null,
                },
            };
            engine.Commit(null, null, null, null, pks);
        }
        static void PkViolation(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 31);
            var pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g1,
                    AggregateTypeId = "pk.violation.1",
                    PrimaryKey = "key.will.be.violated",
                },
            };
            engine.Commit(null, null, null, null, pks);

            pks = new List<AggregatePrimaryKeyRecord>()
            {
                new AggregatePrimaryKeyRecord()
                {
                    AggregateId = g2,
                    AggregateTypeId = "pk.violation.1",
                    PrimaryKey = "key.will.be.violated",
                },
            };
            var exception = false;
            try
            {
                engine.Commit(null, null, null, null, pks);
            }
            catch(AggregatePrimaryKeyViolationException)
            {
                //ignore, it is expected!
                exception = true;
            }
            exception.ShouldBeEquivalentTo(true, "AggregatePrimaryKeyViolationException is excepted.");
        }
    }
}

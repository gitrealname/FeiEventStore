using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    TimeIt("EventsStoreVersionViolation", () => EventsStoreVersionViolation(engine));
                    TimeIt("EventsAggregateVersionViolation", () => EventsAggregateVersionViolation(engine));

                    TimeIt("SnapshotInsert", () => SnapshotInsert(engine));
                    TimeIt("SnapshotUpdate", () => SnapshotUpdate(engine));

                    TimeIt("ProcessInsert", () => ProcessInsert(engine));
                    TimeIt("ProcessUpdate", () => ProcessUpdate(engine));
                    TimeIt("ProcessDelete", () => ProcessDelete(engine));
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
            Console.WriteLine($"{name}: {sw.ElapsedMilliseconds}/{sw.ElapsedTicks} msc/ticks.");
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
        static void ProcessInsert(SqlPersistenceEngine engine, byte id = 1)
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
                    ProcessTypeId = "process.type.1",
                    ProcessStateTypeId = "process.state.type.1",
                    State = @"{""val"":""process.state.1""}",
                },
                new ProcessRecord()
                {
                    ProcessId = g1,
                    InvolvedAggregateId = g2,
                    ProcessVersion = 1,
                    ProcessTypeId = "process.type.1",
                    ProcessStateTypeId = null,
                    State = null,
                },
            };
            engine.Commit(null, null, processes, null, null);
        }

        static void SnapshotInsert(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
            var snapshots = new List<SnapshotRecord>()
            {
                new SnapshotRecord()
                {
                    AggregateId = g1,
                    AggregateVersion = 100,
                    AggregateTypeId = "snapshot.insert.aggregate.type.1",
                    AggregateStateTypeId = "snapshot.insert.state.type.1",
                    State = @"{""val"":""state.1""}",
                },
                new SnapshotRecord()
                {
                    AggregateId = g2,
                    AggregateVersion = 200,
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

        static void EventsInsert(SqlPersistenceEngine engine)
        {
            var g1 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
            var g2 = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
            var events = new List<EventRecord>()
            {
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 1,
                    AggregateId = g1,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.insert.1",
                    EventPayloadTypeId = "payload.type.1",
                    Payload = @"{""val"":""payload.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 2,
                    AggregateId = g1,
                    AggregateVersion = 2,
                    AggregateTypeId = "event.insert.1",
                    EventPayloadTypeId = "payload.type.2",
                    Payload = @"{""val"":""payload.2""}", AggregateTypeUniqueKey = null, OriginUserId = null, EventTimestamp = DateTimeOffset.UtcNow,
                },
                new EventRecord()
                {
                    StoreVersion = engine.StoreVersion + 3,
                    AggregateId = g2,
                    AggregateVersion = 1,
                    AggregateTypeId = "event.insert.1",
                    EventPayloadTypeId = "payload.type.1",
                    Payload = @"{""val"":""payload.3""}", AggregateTypeUniqueKey = null, OriginUserId = "user.1", EventTimestamp = DateTimeOffset.UtcNow,
                },
            };
            var expectedVersion = engine.StoreVersion + 3;
            var ver = engine.Commit(events, null, null, null, null);
            ver.ShouldBeEquivalentTo(expectedVersion);
        }

        static void EventsStoreVersionViolation(SqlPersistenceEngine engine)
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
            try
            {
                engine.Commit(events, null, null, null, null);
            }
            catch(EventStoreConcurrencyViolationException)
            {
                //ignore, is expected
            }
            engine.StoreVersion.ShouldBeEquivalentTo(expectedVersion);
        }
        static void EventsAggregateVersionViolation(SqlPersistenceEngine engine)
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
            try
            {
                engine.Commit(events, null, null, null, null);
            }
            catch(AggregateConcurrencyViolationException)
            {
                //ignore, is expected
            }
            engine.StoreVersion.ShouldBeEquivalentTo(expectedVersion);
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
            try
            {
                engine.Commit(null, null, null, null, pks);
            }
            catch(AggregatePrimaryKeyViolationException)
            {
                //ignore, it is expected!
            }
        }
    }
}

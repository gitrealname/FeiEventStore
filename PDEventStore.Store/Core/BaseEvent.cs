namespace PDEventStore.Store.Core
{
    using System;

    public class BaseEvent : IEvent
    {
        protected class HeaderInfo
        {
            public MessageOrigin Origin;
            public Guid? ProcessId;
            public long StoreVersion;
            public AggregateVersion SourceAggregateVersion;
            public Guid SourceAggregateTypeId;
            public string AggregateKey;
            public DateTimeOffset Timestapm;
        }

        protected HeaderInfo Header;

        public object BackupAndClearTransientState()
        {
            return Header;
        }

        public void RestoreTransientInfoFromBackup(object backup)
        {
            Header = (HeaderInfo)backup;
        }

        public MessageOrigin Origin { get { return Header.Origin;  } set { Header.Origin = value; } }
        public Guid? ProcessId { get { return Header.ProcessId;  } set { Header.ProcessId = value; } }

        public long StoreVersion { get { return Header.StoreVersion; } set { Header.StoreVersion = value; } }
        public AggregateVersion SourceAggregateVersion { get { return Header.SourceAggregateVersion; } set { Header.SourceAggregateVersion = value; } }
        public Guid SourceAggregateTypeId { get { return Header.SourceAggregateTypeId; } set { Header.SourceAggregateTypeId = value; } }
        public string AggregateKey { get { return Header.AggregateKey; } set { Header.AggregateKey = value; } }
        public DateTimeOffset Timestapm { get { return Header.Timestapm; } set { Header.Timestapm = value; } }

        public BaseEvent()
        {
            Header.Timestapm = DateTimeOffset.UtcNow;
        }
    }
}

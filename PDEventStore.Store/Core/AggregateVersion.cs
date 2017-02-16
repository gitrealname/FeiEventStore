namespace PDEventStore.Store.Core
{
    using System;

    public class AggregateVersion : IEquatable<AggregateVersion>
    {
        public AggregateVersion(Guid aggrigateId, long version)
        {
            Id = aggrigateId;
            Version = version;
        }
        public Guid Id { get; private set; }
        public long Version { get; private set; }

        public bool Equals(AggregateVersion other)
        {
            return object.ReferenceEquals(this, other) || (this.Id == other.Id && this.Version == other.Version);
        }

        public override bool Equals(object obj)
        {
            return (obj != null && obj.GetType() == typeof(AggregateVersion) && this.Equals((AggregateVersion)obj));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Version.GetHashCode();
        }

    }
}

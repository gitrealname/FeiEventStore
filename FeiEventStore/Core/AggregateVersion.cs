namespace FeiEventStore.Core
{
    using System;

    public class AggregateVersion : BaseValueObject<AggregateVersion>
    {
        public AggregateVersion(Guid aggrigateId, long version)
        {
            Id = aggrigateId;
            Version = version;
        }
        public Guid Id { get; private set; }
        public long Version { get; private set; }

        protected override bool EqualsCore(AggregateVersion other)
        {
            return this.Id == other.Id && this.Version == other.Version;
        }

        protected override int GetHashCodeCore()
        {
            return (Id.GetHashCode() * 397) ^ Version.GetHashCode();
        }
    }
}

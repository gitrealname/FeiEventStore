namespace FeiEventStore.Core
{
    using System;

    public sealed class MessageOrigin : BaseValueObject<MessageOrigin>
    {
        public Guid? SystemId { get; private set; }
        public Guid? UserId { get; private set; }

        public MessageOrigin(MessageOrigin other) : this(other.SystemId, other.UserId)
        {
        }

        public MessageOrigin(Guid? systemId, Guid? userId)
        {
            SystemId = systemId;
            UserId = userId;
        }

        protected override bool EqualsCore(MessageOrigin other)
        {
            return this.SystemId == other.SystemId && this.UserId == other.UserId;
        }

        protected override int GetHashCodeCore()
        {
            return (SystemId.GetHashCode() * 397) ^ UserId.GetHashCode();
        }
    }

}

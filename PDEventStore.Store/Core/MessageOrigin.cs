namespace PDEventStore.Store.Core
{
    using System;

    public sealed class MessageOrigin : IEquatable<MessageOrigin>
    {
        public MessageOrigin(MessageOrigin other) : this(other.SystemId, other.UserId)
        {
        }

        public MessageOrigin(Guid? systemId, Guid? userId)
        {
            SystemId = systemId;
            UserId = userId;
        }

        public Guid? SystemId { get; private set; }
        public Guid? UserId { get; private set; }

        public bool Equals(MessageOrigin other)
        {
            return object.ReferenceEquals(this, other) || (this.SystemId == other.SystemId && this.UserId == other.SystemId);
        }

        public override bool Equals(object obj)
        {
            return (obj != null && obj.GetType() == typeof(MessageOrigin) && this.Equals((MessageOrigin)obj));
        }

        public override int GetHashCode()
        {
            return SystemId.GetHashCode() ^ UserId.GetHashCode();
        }
    }

}

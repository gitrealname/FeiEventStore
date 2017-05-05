namespace FeiEventStore.Core
{
    using System;

    public sealed class MessageOrigin : BaseValueObject<MessageOrigin>
    {
        public string SystemId { get; private set; }
        public string UserId { get; private set; }

        public MessageOrigin(MessageOrigin other) : this(other.SystemId, other.UserId)
        {
            
        }

        public MessageOrigin(string systemId, string userId)
        {
            if(systemId == null && userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "Either System Id or User id or both must be specified");                
            }

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

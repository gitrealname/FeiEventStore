using System;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public class UserEMessageTbl
    {
        public UserEMessageTbl()
        {
        }
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid EMessageId { get; set; }

        public bool IsRead { get; set; }

        public bool Flagged { get; set; }

        public string FolderTag { get; set; } //inbox, deleted, etc

    }
}
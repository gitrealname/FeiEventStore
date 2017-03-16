using System;

namespace EventStoreIntegrationTester.EventQueues.Ado.DbModel
{
    public class UserGroupTbl
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; }

        public Guid? CounterId { get; set; }
    }
}

using System;

namespace FeiEventStore.IntegrationTests.EventQueues.Ado.DbModel
{
    public class UserGroupTbl
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; }

        public Guid? CounterId { get; set; }
    }
}

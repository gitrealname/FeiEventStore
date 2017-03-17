using System;

namespace FeiEventStore.IntegrationTests._Tests
{
    public static class Const
    {
        public static Guid FirstCounterId = new Guid("{00000000-0000-0000-0000-000000000001}");
        public static Guid FirstUserGroup = new Guid("{00000000-0000-0000-0000-000000000010}");
        public static Guid SecondUserGroup = new Guid("{00000000-0000-0000-0000-000000000020}");
        public static Guid? OriginSystemId = new Guid("{00000000-0000-0000-0001-000000000000}");

        public static Guid EMessageId = new Guid("{00000000-0000-0001-0000-000000000001}");
        public static Guid UserId1 =    new Guid("{00000000-0001-0000-0000-000000000001}");
        public static Guid UserId2 =    new Guid("{00000000-0002-0000-0000-000000000002}");
        public static Guid UserId3 =    new Guid("{00000000-0003-0000-0000-000000000003}");
    }
}
using System;

namespace FeiEventStore.IntegrationTests._Tests
{
    public static class Const
    {
        public static Guid FirstCounterId = new Guid("{00000000-0000-0000-0000-000000000001}");
        public static Guid FirstUserGroup = new Guid("{00000000-0000-0000-0000-000000000010}");
        public static Guid SecondUserGroup = new Guid("{00000000-0000-0000-0000-000000000020}");
        public static string OriginSystemId = "FeiEventStore.IntegrationTests";

        public static Guid EMessageId = new Guid("{00000000-0000-0001-0000-000000000001}");
        public static string UserId1 = "u1";
        public static string UserId2 = "u2";
        public static string UserId3 = "u3";
    }
}
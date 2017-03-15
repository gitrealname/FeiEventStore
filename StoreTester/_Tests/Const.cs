using System;

namespace EventStoreIntegrationTester._Tests
{
    public static class Const
    {
        public static Guid FirstCounterId = new Guid("{00000000-0000-0000-0000-000000000001}");
        public static Guid FirstUserGroup = new Guid("{00000000-0000-0000-0000-000000000010}");
        public static Guid SecondUserGroup = new Guid("{00000000-0000-0000-0000-000000000020}");
        public static Guid? OriginSystemId = new Guid("{00000000-0000-0000-0001-000000000000}");
    }
}
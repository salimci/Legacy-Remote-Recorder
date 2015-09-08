using RemoteRecorderTest.Enum;

namespace RemoteRecorderTest
{
    /// <summary>
    /// Set test configuration before run test program
    /// </summary>
    public static class TestConfig
    {
        public static TestInputMode InputMode = TestInputMode.FromFile;
        public static TestOutputMode OutputMode = TestOutputMode.ToDb;
    }
}

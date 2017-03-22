namespace DistCommon
{
    public static class Constants
    {
        public static class Comm
        {
            public const int StreamSize = 512;
        }

        public static class Node
        {
            public const string ConfigFilename = "DistNode.cfg";
            public static class Worker
            {
                public const string ProcessFilename = "cmd.exe";
                public const string CmdPrefix = "/C ";
                public const int TimerInitialWait = 1000;
                public const int TimerPeriod = 1000;
            }
        }
    }
}

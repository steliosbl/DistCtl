namespace DistCommon
{
    public static class Constants
    {
        public static class Comm
        {
            public const int StreamSize = 512;
            public const string InvalidResponse = "NULL";
        }

        public static class Node
        {
            public const string ConfigFilename = "DistNode.cfg";
            public const string LogFilename = "DistNode.log";

            public static class Worker
            {
                public const string ProcessFilename = "cmd.exe";
                public const string CmdPrefix = "/C ";
                public const int CheckDelay = 500;
            }
        }

        public static class Ctl
        {
            public const int RequestAttempts = 3;
            public const string ConfigFilename = "DistCtl.cfg";
            public const string LogFilename = "DistCtl.log";
        }

        public static class Logger
        {
            public static string[] BannedStrings = { "Application started. Press Ctrl+C to shut down." };
        }
    }
}

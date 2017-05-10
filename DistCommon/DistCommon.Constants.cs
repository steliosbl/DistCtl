namespace DistCommon
{
    public static class Constants
    {
        public static class Results
        {
            public static readonly string[] Messages = { "Fail", "Success", "Not Found", "Invalid", "Not Constructed", "Unreachable" };
        }

        public static class Logger
        {
            public static readonly System.ConsoleColor[] Colors = { System.ConsoleColor.White, System.ConsoleColor.Yellow, System.ConsoleColor.Red, System.ConsoleColor.Black };
        }

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
    }
}

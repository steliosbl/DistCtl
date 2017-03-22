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
            }
        }
    }
}

namespace DistCommon.Constants
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

        public static class Results
        {
            public const int Fail = 0;
            public const int Success = 1;
            public const int NotFound = 2;
            public const int Invalid = 3;
            public const int NotConstructed = 4;
            public static string[] Message = { "Fail", "Success", "Not Found", "Invalid", "Not Constructed"};
        }
}

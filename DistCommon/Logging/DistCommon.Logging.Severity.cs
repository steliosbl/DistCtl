namespace DistCommon.Logging
{
    public enum Severity
    {
        Info,
        Warn,
        Severe,
        Critical
    }

    public static class SeverityInfo
    {
        public static string GetTag(this Severity severity)
        {
            switch (severity)
            {
                case Severity.Info:
                    return "INFO";
                case Severity.Warn:
                    return "WARN";
                case Severity.Severe:
                    return "SEVERE";
                case Severity.Critical:
                    return "CRITICAL";
                default:
                    return "UNKNOWN";
            }
        }

        public static System.ConsoleColor GetColor(this Severity severity)
        {
            switch (severity)
            {
                case Severity.Info:
                    return System.ConsoleColor.White;
                case Severity.Warn:
                    return System.ConsoleColor.Yellow;
                case Severity.Severe:
                    return System.ConsoleColor.Red;
                case Severity.Critical:
                    return System.ConsoleColor.DarkRed;
                default:
                    return System.ConsoleColor.Magenta;
            }
        }
    }
}

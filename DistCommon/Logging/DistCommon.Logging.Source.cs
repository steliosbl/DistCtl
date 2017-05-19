namespace DistCommon.Logging
{
    public enum Source
    {
        Ctl,
        Console,
        API,
        Node,
        Runtime
    }

    public static class SourceInfo
    {
        public static string GetName(this Source src)
        {
            switch (src)
            {
                case Source.Ctl:
                    return "Ctl";
                case Source.Console:
                    return "Console";
                case Source.API:
                    return "API";
                case Source.Node:
                    return "Node";
                case Source.Runtime:
                    return "Bootstrap";
                default:
                    return "Unknown";
            }
        }
    }
}

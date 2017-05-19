namespace DistCommon.Logging
{
    public interface ILogger
    {
        void Log(string msg);

        void Log(string msg, Source src);

        void Log(string msg, Severity severity);

        void Log(string msg, Source src, Severity severity);
    }
}

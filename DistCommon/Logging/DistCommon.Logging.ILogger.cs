namespace DistCommon.Logging
{
    public interface ILogger
    {
        int AddID();

        void SubtractID();

        void Log(string msg);

        void Log(string msg, int id);

        void Log(string msg, Source src);

        void Log(string msg, Source src, int id);

        void Log(string msg, Severity severity);

        void Log(string msg, Severity severity, int id);

        void Log(string msg, Source src, Severity severity);

        void Log(string msg, Source src, Severity severity, int id);
    }
}

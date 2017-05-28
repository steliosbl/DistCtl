namespace DistCommon.Logging
{
    using System;
    using System.IO;
    using System.Text;

    public sealed class LogWriter : TextWriter
    {
        private ILogger logger;

        public LogWriter(ILogger logger)
        {
            this.logger = logger;
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.GetEncoding(0);
            }
        }

        public override void WriteLine(string s)
        {
            this.logger.Log(s);
        }

        public override void Write(char value)
        {
        }
    }
}

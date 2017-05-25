namespace DistCommon.Logging
{
    using System;
    using System.IO;

    public class Logger : ILogger
    {
        #region Fields
        private readonly string filename;
        private SayHandler say;
        private Source defaultSrc;
        private int currentID;
        #endregion

        #region Constructors
        public Logger(string filename, Source src) : this(filename, src, StdSay)
        {
        }

        public Logger(string filename, Source src, SayHandler sayHandler)
        {
            this.defaultSrc = src;
            this.filename = filename;
            this.Write('\n' + "----------------------------------" + '\n');
            this.say = sayHandler;
            this.currentID = 0;
        }
        #endregion

        #region Delegates
        public delegate void SayHandler(string msg,  ConsoleColor foregroundColor);
        #endregion

        #region Static
        public static void StdSay(string msg, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
        #endregion

        #region Exposed
        public int AddID()
        {
            this.currentID += 1;
            return this.currentID;
        }

        public void SubtractID()
        {
            this.currentID -= 1;
        }

        public void Log(string msg)
        {
            this.Log(msg, this.defaultSrc, 0, 0);
        }

        public void Log(string msg, int id)
        {
            this.Log(msg, this.defaultSrc, 0, id);
        }

        public void Log(string msg, Source src)
        {
            this.Log(msg, src, 0, 0);
        }

        public void Log(string msg, Source src, int id)
        {
            this.Log(msg, src, 0, 0);
        }

        public void Log(string msg, Severity severity)
        {
            this.Log(msg, this.defaultSrc, severity, 0);
        }

        public void Log(string msg, Severity severity, int id)
        {
            this.Log(msg, this.defaultSrc, severity, id);
        }

        public void Log(string msg, Source src, Severity severity)
        {
            this.Log(msg, src, severity, 0);
        }

        public void Log(string msg, Source src, Severity severity, int id)
        {
            string idMsg = string.Empty;
            if (id != 0)
            {
                idMsg = string.Format("[{0}] ", id.ToString());
            }

            string message = string.Format("[{0}] [{1}] [{2}] {3}{4}", DateTime.Now.ToString(), src, severity.GetTag(), idMsg, msg);

            this.Write(message + '\n');
            this.say(message, severity.GetColor());
        }
        #endregion

        #region Internal
        private void Write(string message)
        {
            try
            {
                File.AppendAllText(this.filename, message);
            }
            catch (IOException)
            {
            }
        }
        #endregion
    }
}

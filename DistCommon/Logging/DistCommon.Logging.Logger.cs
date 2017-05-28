namespace DistCommon.Logging
{
    using System;
    using System.IO;
    using System.Linq;

    public class Logger : ILogger
    {
        #region Fields
        private readonly string filename;
        private SayHandler sayHandler;
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
            this.sayHandler = sayHandler;
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
            this.ActualLog(msg, this.defaultSrc, 0, 0);
        }

        public void Log(string msg, int id)
        {
            this.ActualLog(msg, this.defaultSrc, 0, id);
        }

        public void Log(string msg, Source src)
        {
            this.ActualLog(msg, src, 0, 0);
        }

        public void Log(string msg, Source src, int id)
        {
            this.ActualLog(msg, src, 0, 0);
        }

        public void Log(string msg, Severity severity)
        {
            this.ActualLog(msg, this.defaultSrc, severity, 0);
        }

        public void Log(string msg, Severity severity, int id)
        {
            this.ActualLog(msg, this.defaultSrc, severity, id);
        }

        public void Log(string msg, Source src, Severity severity)
        {
            this.ActualLog(msg, src, severity, 0);
        }

        public void Log(string msg, Source src, Severity severity, int id)
        {
            this.ActualLog(msg, src, severity, id);
        }
        #endregion

        #region Internal
        private void ActualLog(string msg, Source src, Severity severity, int id)
        {
            if (!DistCommon.Constants.Logger.BannedStrings.Contains(msg))
            {
                string idMsg = string.Empty;
                if (id != 0)
                {
                    idMsg = string.Format("[{0}] ", id.ToString());
                }

                string message = string.Format("[{0}] [{1}] [{2}] {3}{4}", DateTime.Now.ToString(), src, severity.GetTag(), idMsg, msg);

                this.Write(message + '\n');
                this.Say(message, severity.GetColor());
            }
        }

        private void Say(string msg, ConsoleColor color)
        {
            var temp = Console.Out;
            using (var stdout = new System.IO.StreamWriter(Console.OpenStandardOutput()))
            {
                Console.SetOut(stdout);
                stdout.AutoFlush = true;
                this.sayHandler(msg, color);
            }

            Console.SetOut(temp);
        }

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

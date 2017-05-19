namespace DistCommon.Logging
{
    using System;
    using System.IO;

    public class Logger
    {
        private readonly string filename;
        private SayHandler say;
        private Source defaultSrc;

        public Logger(string filename, Source src) : this(filename, src, StdSay)
        {
        }

        public Logger(string filename, Source src, SayHandler sayHandler)
        {
            this.defaultSrc = src;
            this.filename = filename;
            this.Write('\n' + "----------------------------------" + '\n');
            this.say = sayHandler;
        }

        public delegate void SayHandler(string msg,  ConsoleColor foregroundColor);

        public static void StdSay(string msg, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public void Log(string msg)
        {
            this.Log(msg, this.defaultSrc);
        }

        public void Log(string msg, Source src)
        {
            this.Log(msg, src, 0);
        }

        public void Log(string msg, Severity severity)
        {
            this.Log(msg, this.defaultSrc, severity);
        }

        public void Log(string msg, Source src, Severity severity)
        {
            string message = string.Format("[{0}] [{1}] [{2}] {3}", DateTime.Now.ToString(), src, severity.GetTag(), msg);

            this.Write(message + '\n');
            this.say(message, severity.GetColor());
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
    }
}

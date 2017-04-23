namespace DistCommon
{
    using System;
    using System.IO;

    public class Logger
    {
        private readonly string filename;
        private SayHandler say;

        public Logger(string filename) : this(filename, StdSay)
        {
        }

        public Logger(string filename, SayHandler sayHandler)
        {
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

        public void Log(string msg, int severity = 0)
        {
            var tags = new string[] { "[INFO]", "[WARN]", "[SEVERE]", "[CRITICAL]" };

            string message = "[" + DateTime.Now.ToString() + "] " + tags[severity] + " " + msg;

            this.Write(message + '\n');
            this.say(message, Constants.Logger.Colors[severity]);
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

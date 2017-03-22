namespace DistCommon
{
    using System;
    using System.IO;

    public class Logger
    {
        private readonly string filename;
        private SayHandler Say;

        public Logger(string filename) : this(filename, Console.WriteLine)
        {
        }

        public Logger(string filename, SayHandler sayHandler)
        {
            this.filename = filename;
            this.Write('\n' + "----------------------------------" + '\n');
            this.Say = sayHandler;
        }

        public delegate void SayHandler(string msg);
        public void Log(string msg, int severity = 0)
        {
            var tags = new string[] { "[INFO]", "[WARN]", "[SEVERE]", "[CRITICAL]" };

            string message = "[" + DateTime.Now.ToString() + "] " + tags[severity] + " " + msg;

            this.Write(message + '\n');
            this.Say(message);
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

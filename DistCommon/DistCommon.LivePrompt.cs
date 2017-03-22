namespace DistCommon
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class LivePrompt
    {
        private object locker;
        private List<char> buffer;
        private string prompt = ">";
        private Thread loopThread;

        public LivePrompt(InputReceivedHandler inputHandler)
        {
            this.locker = new object();
            this.buffer = new List<char>();
            this.buffer.AddRange(this.prompt);
            Console.Write(new string(this.buffer.ToArray()));
            this.InputReceived += inputHandler;
            this.loopThread = new Thread(() => this.MainLoop());
            this.loopThread.Start();
        }

        public delegate void InputReceivedHandler(string input);

        public event InputReceivedHandler InputReceived;

        public void MainLoop()
        {
            while (true)
            {
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.Enter && this.buffer.Count > 0)
                {
                    lock (this.locker)
                    {
                        Console.WriteLine();
                        this.buffer.Clear();
                        this.buffer.AddRange(this.prompt);
                        Console.Write(this.buffer.ToArray());
                        this.InputReceived(this.buffer.ToString());
                    }
                }
                else
                {
                    this.buffer.Add(k.KeyChar);
                }
            }
        }

        public void Say(string msg)
        {
            lock (this.locker)
            {
                Console.Write(new string('\b', this.buffer.Count));
                var excess = this.buffer.Count - msg.Length;
                if (excess > 0)
                {
                    msg += new string(' ', excess);
                }

                Console.WriteLine(msg);
                Console.Write(new string(this.buffer.ToArray()));
            }
        }
    }
}

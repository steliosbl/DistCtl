namespace DistCommon
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public sealed class LivePrompt
    {
        private object locker;
        private List<char> buffer;
        private string prompt = "> ";
        ////private Thread loopThread;

        public LivePrompt()
        {
            this.locker = new object();
            this.buffer = new List<char>();
            this.buffer.AddRange(this.prompt);
            Console.Write(new string(this.buffer.ToArray()));
            ////this.loopThread = new Thread(() => this.MainLoop());
            ////this.loopThread.Start();
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
                        var temp = new List<char>(this.buffer);
                        this.buffer.Clear();
                        this.buffer.AddRange(this.prompt);
                        temp.RemoveRange(0, this.prompt.Length);
                        this.OnInputReceived(new string(temp.ToArray()));
                    }
                }
                else if (k.Key == ConsoleKey.Backspace && this.buffer.Count > this.prompt.Length)
                {
                    this.buffer.RemoveAt(this.buffer.Count - 1);
                    lock (this.locker)
                    {
                        Console.Write(" \b");
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

        public void AddInputHandler(InputReceivedHandler inputHandler)
        {
            this.InputReceived += inputHandler;
        }

        private void OnInputReceived(string input)
        {
            if (this.InputReceived != null)
            {
                this.InputReceived(input);
            }
        }
    }
}

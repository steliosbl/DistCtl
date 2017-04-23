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
        private int historyIndex = 0;
        private List<string> history;
        ////private Thread loopThread;

        public LivePrompt()
        {
            this.locker = new object();
            this.buffer = new List<char>();
            this.history = new List<string>();
            this.history.Add(string.Empty);
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
                        string tempstr = new string(temp.ToArray());
                        this.OnInputReceived(tempstr);
                        this.history.Insert(1, tempstr);
                    }
                }
                else if (k.Key == ConsoleKey.Backspace)
                {
                    if (this.buffer.Count > this.prompt.Length)
                    {
                        this.buffer.RemoveAt(this.buffer.Count - 1);
                        lock (this.locker)
                        {
                            Console.Write(" \b");
                        }
                    }
                }
                else if (k.Key == ConsoleKey.UpArrow)
                {
                    if (this.historyIndex < this.history.Count - 1)
                    {
                        this.historyIndex += 1;
                        this.ShowHistory(this.historyIndex);
                    }
                }
                else if (k.Key == ConsoleKey.DownArrow)
                {
                    if (this.historyIndex > 0)
                    {
                        this.historyIndex -= 1;
                        this.ShowHistory(this.historyIndex);
                    }
                }
                else
                {
                    this.buffer.Add(k.KeyChar);
                }
            }
        }

        public void Say(string msg, ConsoleColor foregroundColor = ConsoleColor.White)
        {
            lock (this.locker)
            {
                Console.Write(new string('\b', this.buffer.Count));
                var excess = this.buffer.Count - msg.Length;
                if (excess > 0)
                {
                    msg += new string(' ', excess);
                }

                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(msg);
                Console.ResetColor();
                Console.Write(new string(this.buffer.ToArray()));
            }
        }

        public void AddInputHandler(InputReceivedHandler inputHandler)
        {
            this.InputReceived += inputHandler;
        }

        private static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private void ShowHistory(int index)
        {
            ClearCurrentConsoleLine();
            this.buffer.Clear();
            this.buffer.AddRange(this.prompt.ToCharArray());
            this.buffer.AddRange(this.history[index].ToCharArray());
            Console.Write(new string(this.buffer.ToArray()));
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

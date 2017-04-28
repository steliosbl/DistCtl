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
        private int cursorLeft;
        private bool cursorMoved;
        ////private Thread loopThread;

        public LivePrompt()
        {
            this.locker = new object();
            this.buffer = new List<char>();
            this.history = new List<string>();
            this.cursorLeft = this.prompt.Length;
            this.cursorMoved = false;
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
                var k = Console.ReadKey(true);
                if (k.Key == ConsoleKey.Enter && this.buffer.Count > 0)
                {
                    this.cursorLeft = this.prompt.Length;
                    this.cursorMoved = false;
                    this.historyIndex = 0;
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
                    if (this.cursorLeft > this.prompt.Length)
                    {
                        this.buffer.RemoveAt(this.cursorLeft - 1);
                        this.cursorLeft -= 1;
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
                else if (k.Key == ConsoleKey.LeftArrow)
                {
                    if (this.cursorLeft > this.prompt.Length - 1)
                    {
                        this.cursorMoved = true;
                        this.cursorLeft -= 1;
                    }
                }
                else if (k.Key == ConsoleKey.RightArrow)
                {
                    if (this.cursorLeft < this.buffer.Count)
                    {
                        this.cursorLeft += 1;
                    }
                }
                else
                {
                    this.buffer.Insert(this.cursorLeft, k.KeyChar);
                    if (this.cursorMoved)
                    {
                        this.cursorLeft += 1;
                    }
                }

                if (!this.cursorMoved)
                {
                    this.cursorLeft = this.buffer.Count;
                }

                ClearCurrentConsoleLine();
                Console.Write(new string(this.buffer.ToArray()));
                Console.SetCursorPosition(this.cursorLeft, Console.CursorTop);
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

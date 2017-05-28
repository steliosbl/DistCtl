namespace DistCommon.Utils
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class ThreadAwareStreamWriter : TextWriter
    {
        private ConcurrentDictionary<int, TextWriter> threadWriterMap;
        private TextWriter defaultWriter;

        public ThreadAwareStreamWriter()
        {
            this.threadWriterMap = new ConcurrentDictionary<int, TextWriter>();
            this.defaultWriter = Console.Out;
        }

        public override void Write(char value)
        {
            TextWriter threadWriter;
            if (this.threadWriterMap.TryGetValue(Thread.CurrentThread.ManagedThreadId, out threadWriter))
            {
                threadWriter.Write(value);
                return;
            }

            if (this.defaultWriter != null)
            {
                this.defaultWriter.Write(value);
            }
        }

        public TextWriter RegisterThreadWriter(TextWriter threadWriter)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            TextWriter oldWriter;
            this.threadWriterMap.TryGetValue(threadId, out oldWriter);
            this.threadWriterMap[threadId] = threadWriter;

            return oldWriter;
        }

        public void DeregisterThread()
        {
            TextWriter threadWriter;
            if (this.threadWriterMap.TryRemove(Thread.CurrentThread.ManagedThreadId, out threadWriter))
            {
                threadWriter.Dispose();
            }
        }

        public override Encoding Encoding
        {
            get
            {
                TextWriter threadWriter;
                if (this.threadWriterMap.TryGetValue(Thread.CurrentThread.ManagedThreadId, out threadWriter))
                {
                    return threadWriter.Encoding;
                }

                return this.defaultWriter.Encoding;
            }
        }

        public override void WriteLine(string value)
        {
            TextWriter threadWriter;
            if (this.threadWriterMap.TryGetValue(Thread.CurrentThread.ManagedThreadId, out threadWriter))
            {
                threadWriter.WriteLine(value);
                return;
            }

            if (this.defaultWriter != null)
            {
                this.defaultWriter.WriteLine(value);
            }
        }
    }
}

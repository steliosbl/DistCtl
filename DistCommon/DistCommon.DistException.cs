namespace DistCommon
{
    using System;

    public sealed class DistException : Exception
    {
        public DistException() : base()
        {
        }

        public DistException(string message) : base(message)
        {
        }

        public DistException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

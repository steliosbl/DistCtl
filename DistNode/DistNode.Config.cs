namespace DistNode
{
    internal sealed class Config
    {
        public readonly int Port;
        public readonly bool LiveErrors;

        [Newtonsoft.Json.JsonConstructor]
        public Config(int port, bool liveErrors)
        {
            this.Port = port;
            this.LiveErrors = liveErrors;
        }
    }
}

namespace DistNode
{
    internal sealed class Config
    {
        public readonly int Port;
        public readonly bool EnableLiveErrors;

        [Newtonsoft.Json.JsonConstructor]
        public Config(int port, bool enableLiveErrors)
        {
            this.Port = port;
            this.EnableLiveErrors = enableLiveErrors;
        }
    }
}

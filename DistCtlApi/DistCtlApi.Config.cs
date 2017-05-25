namespace DistCtlApi
{
    public sealed class Config
    {
        public readonly bool Enable;
        public readonly int Port;

        [Newtonsoft.Json.JsonConstructor]
        public Config(bool enable, int port)
        {
            this.Enable = enable;
            this.Port = port;
        }
    }
}

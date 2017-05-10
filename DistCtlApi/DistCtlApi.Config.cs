namespace DistCtlApi
{
    public sealed class Config
    {
        public readonly bool Enable;

        [Newtonsoft.Json.JsonConstructor]
        public Config(bool enable)
        {
            this.Enable = enable;
        }
    }
}

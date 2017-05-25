namespace DistCtlRuntime
{
    public sealed class Config
    {
        public readonly DistCtl.Config CtlConfig;
        public readonly DistCtlApi.Config APIConfig;
        public readonly bool EnableLocalConsole;
        public readonly bool EnableLiveErrors;

        [Newtonsoft.Json.JsonConstructor]
        public Config(DistCtl.Config ctlConfig, DistCtlApi.Config apiConfig, bool enableLocalConsole, bool enableAPI, bool enableLiveErrors)
        {
            this.CtlConfig = ctlConfig;
            this.APIConfig = apiConfig;
            this.EnableLocalConsole = enableLocalConsole;
            this.EnableLiveErrors = enableLiveErrors;
        }
    }
}

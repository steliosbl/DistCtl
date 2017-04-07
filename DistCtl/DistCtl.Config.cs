namespace DistCtl
{
    public sealed class Config
    {
        public readonly bool EnableLiveErrors;
        public readonly bool EnableLocalConsole;
        public readonly bool EnableJobPreload;
        public readonly bool EnableRedundancy;
        public readonly bool EnableLoadBalancing;
        public readonly bool EnableAutoRestart;
        public readonly bool EnableRejectTooManyAssignments;
        public readonly int UpdateDelay;
        public readonly int TimeoutDuration;
        public readonly string SchematicFilename;
        public readonly string PreLoadFilename;

        [Newtonsoft.Json.JsonConstructor]
        public Config(bool enableLiveErrors, bool enableLocalConsole, bool enableJobPreload, bool enableRedundancy, bool enableLoadBalancng, bool enableAutoRestart, bool enableRejectTooManyAssignments, int updateDelay, int timeoutDuration, string schematicFilename, string preLoadFilename)
        {
            this.EnableLiveErrors = enableLiveErrors;
            this.EnableLocalConsole = enableLocalConsole;
            this.EnableJobPreload = enableJobPreload;
            this.EnableRedundancy = enableRedundancy;
            this.EnableLoadBalancing = enableLoadBalancng;
            this.EnableAutoRestart = enableAutoRestart;
            this.EnableRejectTooManyAssignments = enableRejectTooManyAssignments;
            this.UpdateDelay = updateDelay;
            this.TimeoutDuration = timeoutDuration;
            this.SchematicFilename = schematicFilename;
            this.PreLoadFilename = preLoadFilename;
        }
    }
}

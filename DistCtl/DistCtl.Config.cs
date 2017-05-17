namespace DistCtl
{
    public sealed class Config
    {
        public readonly bool EnableJobPreload;
        public readonly bool EnableRedundancy;
        public readonly bool EnableLoadBalancing;
        public readonly bool EnableAutoRestart;
        public readonly bool EnableRejectTooManyAssignments;
        public readonly bool EnableWakePreloadJobs;
        public readonly int UpdateDelay;
        public readonly int TimeoutDuration;
        public readonly string SchematicFilename;
        public readonly string PreLoadFilename;

        [Newtonsoft.Json.JsonConstructor]
        public Config(bool enableJobPreload, bool enableRedundancy, bool enableLoadBalancng, bool enableAutoRestart, bool enableRejectTooManyAssignments, bool enableWakePreloadJobs, int updateDelay, int timeoutDuration, string schematicFilename, string preLoadFilename)
        {
            this.EnableJobPreload = enableJobPreload;
            this.EnableRedundancy = enableRedundancy;
            this.EnableLoadBalancing = enableLoadBalancng;
            this.EnableAutoRestart = enableAutoRestart;
            this.EnableRejectTooManyAssignments = enableRejectTooManyAssignments;
            this.EnableWakePreloadJobs = enableWakePreloadJobs;
            this.UpdateDelay = updateDelay;
            this.TimeoutDuration = timeoutDuration;
            this.SchematicFilename = schematicFilename;
            this.PreLoadFilename = preLoadFilename;
        }
    }
}

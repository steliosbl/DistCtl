namespace DistCtl
{
    public sealed class JobInfo
    {
        public readonly DistCommon.Job.Blueprint Blueprint;
        public readonly int NodeID;

        public JobInfo(DistCommon.Job.Blueprint blueprint, int nodeID)
        {
            this.Blueprint = blueprint;
            this.NodeID = nodeID;
        }
    }
}

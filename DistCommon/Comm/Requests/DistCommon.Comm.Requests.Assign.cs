namespace DistCommon.Comm.Requests
{
    public sealed class Assign : Base
    {
        public Assign(Job.Blueprint blueprint) : base()
        {
            this.Blueprint = blueprint;
        }

        public Job.Blueprint Blueprint { get; private set; }
    }
}

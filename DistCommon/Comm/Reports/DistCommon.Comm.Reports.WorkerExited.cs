namespace DistCommon.Comm.Reports
{
    public sealed class WorkerExited : Base
    {
        public WorkerExited(int id) : base()
        {
            this.ID = id;
        }

        public int ID { get; private set; }
    }
}

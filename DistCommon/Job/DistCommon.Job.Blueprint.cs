namespace DistCommon.Job
{
    public sealed class Blueprint
    {
        public readonly int ID;
        public readonly int Priority;
        public readonly string Command;
        
        public Blueprint(int id, int priority, string command)
        {
            this.ID = id;
            this.Priority = priority;
            this.Command = command;
        }
    }
}

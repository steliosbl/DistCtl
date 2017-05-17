namespace DistCommon.Job
{
    public sealed class Blueprint
    {
        public readonly int ID;
        public readonly int? NodeID;
        public readonly int Priority;
        public readonly string Command;
        
        public Blueprint(int id, int? nodeID, int priority, string command)
        {
            this.ID = id;
            this.NodeID = nodeID;
            this.Priority = priority;
            this.Command = command;
        }
    }
}

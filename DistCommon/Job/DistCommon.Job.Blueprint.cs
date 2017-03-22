namespace DistCommon.Job
{
    public sealed class Blueprint
    {
        public readonly int ID;
        public readonly string Command;
        
        public Blueprint(int id, string command)
        {
            this.ID = id;
            this.Command = command;
        }
    }
}

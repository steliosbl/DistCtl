namespace DistCommon.Job
{
    public sealed class Blueprint
    {
        public readonly int ID;
        public readonly string Command;
        public readonly string Arguments;
        
        public Blueprint(int id, string command, string arguments)
        {
            this.ID = id;
            this.Command = command;
            this.Arguments = arguments;
        }
    }
}

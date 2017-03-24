namespace DistCommon.Comm.Requests
{
    public sealed class Construct : Base
    {
        public Construct(DistCommon.Schema.Node schematic) : base()
        {
            this.Schematic = schematic;
        }

        public DistCommon.Schema.Node Schematic { get; private set; }
    }
}

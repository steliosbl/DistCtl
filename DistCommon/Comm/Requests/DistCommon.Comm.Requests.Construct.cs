namespace DistCommon.Comm.Requests
{
    public sealed class Construct : Base
    {
        public Construct(DistCommon.Schematic.Node schematic) : base()
        {
            this.Schematic = schematic;
        }

        public DistCommon.Schematic.Node Schematic { get; private set; }
    }
}

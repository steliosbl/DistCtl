namespace DistCtl
{
    public sealed class NodeInfo
    {
        public readonly DistCommon.Schema.Node Schematic;
        public readonly bool Reachable;

        public NodeInfo(DistCommon.Schema.Node schematic, bool reachable)
        {
            this.Schematic = schematic;
            this.Reachable = reachable;
        }
    }
}
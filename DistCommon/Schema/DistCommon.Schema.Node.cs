namespace DistCommon.Schema
{
    public sealed class Node
    {
        public readonly int ID;
        public readonly int Slots;
        public readonly System.Net.IPEndPoint Address;
        public readonly bool Primary;

        [Newtonsoft.Json.JsonConstructor]
        public Node(int id, int slots, System.Net.IPEndPoint address, bool primary)
        {
            this.ID = id;
            this.Slots = slots;
            this.Address = address;
            this.Primary = primary;
        }

        public bool Validate()
        {
            return this.ID > 0 && this.Slots > 0;
        }
    }
}

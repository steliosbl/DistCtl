namespace DistCommon.Schema
{
    using System;
    using System.Collections.Generic;

    public sealed class Controller
    {
        public readonly int ID;
        public readonly System.Net.IPEndPoint Address;
        public readonly Dictionary<int, Node> Nodes;
        public readonly Dictionary<int, Controller> SubControllers;

        [Newtonsoft.Json.JsonConstructor]
        public Controller(int id, System.Net.IPEndPoint address, Dictionary<int, Node> nodes, Dictionary<int, Controller> subControllers)
        {
            this.ID = id;
            this.Address = address;
            this.Nodes = nodes;
            this.SubControllers = subControllers;
        }
    }
}

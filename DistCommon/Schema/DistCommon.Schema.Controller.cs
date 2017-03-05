namespace DistCommon.Schema
{
    using System;
    using System.Collections.Generic;

    public sealed class Controller
    {
        public readonly int ID;
        public readonly System.Net.IPEndPoint Address;
        public Dictionary<int, Node> Nodes;
        public Dictionary<int, Controller> SubControllers;

        [Newtonsoft.Json.JsonConstructor]
        public Controller(int id, System.Net.IPEndPoint Address, Dictionary<int, Node> nodes, Dictionary<int, Controller> subControllers)
        {
            this.ID = id;
            this.Address = Address;
            this.Nodes = nodes;
            this.SubControllers = subControllers;
        }
    }
}

namespace DistCommon.Schema
{
    using System;
    using System.Collections.Generic;

    public sealed class Schema
    {
        public readonly Dictionary<int, Controller> Controllers;

        [Newtonsoft.Json.JsonConstructor]
        public Schema(Dictionary<int, Controller> controllers)
        {
            this.Controllers = controllers;
        }
    }
}

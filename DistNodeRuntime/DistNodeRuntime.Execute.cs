namespace DistNodeRuntime
{
    public static class Execute
    {
        [System.STAThread]
        public static void Main(string[] args)
        {
            DistNode.Node node;
            Newtonsoft.Json.JsonConvert.DefaultSettings = () => new DistCommon.Serialization.CustomSerializerSettings.SerializerSettings();
            if (args.Length == 0)
            {
                node = new DistNode.Node();
            }
            else
            {
                node = new DistNode.Node(args[0]);
            }
            
            DistCommon.OnExit.Register(() => node.Dispose());
            node.Initialize();
        }
    }
}

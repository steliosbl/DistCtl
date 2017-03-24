namespace DistNodeRuntime
{
    public static class Execute
    {
        [System.STAThread]
        public static void Main(string[] args)
        {
            Newtonsoft.Json.JsonConvert.DefaultSettings = () => new DistCommon.Serialization.CustomSettings();
            if (args.Length == 0)
            {
                new DistNode.Node();
            }
            else
            {
                new DistNode.Node(args[0]);
            }
        }
    }
}

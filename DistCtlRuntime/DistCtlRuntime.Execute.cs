namespace DistCtlRuntime
{
    public static class Execute
    {
        [System.STAThread]
        public static void Main(string[] args)
        {
            Newtonsoft.Json.JsonConvert.DefaultSettings = () => new DistCommon.Serialization.CustomSettings();
            if (args.Length == 0)
            {
                new DistCtl.Controller();
            }
            else
            {
                new DistCtl.Controller(args[0]);
            }
        }
    }
}

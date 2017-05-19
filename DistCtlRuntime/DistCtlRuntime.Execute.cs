namespace DistCtlRuntime
{
    using DistCommon;

    public static class Execute
    {
        [System.STAThread]
        public static void Main(string[] args)
        {
            Newtonsoft.Json.JsonConvert.DefaultSettings = () => new DistCommon.Serialization.CustomSerializerSettings.SerializerSettings();
            Runtime runtime;
            if (args.Length == 0)
            {
                runtime = new DistCtlRuntime.Runtime();
            }
            else
            {
                runtime = new Runtime(args[0]);
            }

            runtime.Start();
        }
    }
}

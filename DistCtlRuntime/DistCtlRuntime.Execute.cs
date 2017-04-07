namespace DistCtlRuntime
{
    using DistCommon;

    public static class Execute
    {
        [System.STAThread]
        public static void Main(string[] args)
        {
            Newtonsoft.Json.JsonConvert.DefaultSettings = () => new DistCommon.Serialization.CustomSettings();

            string configFilename;
            DistCtl.Config config;
            Logger.SayHandler sayHandler;
            LivePrompt prompt;
            DistCtl.Controller controller;

            if (args.Length == 0)
            {
                configFilename = DistCommon.Constants.Ctl.ConfigFilename;
            }
            else
            {
                configFilename = args[0];
            }

            string[] dependencies = { configFilename };
            if (new DepMgr(dependencies).FindMissing().Count != 0)
            {
                throw new DistException("Configuration file not found.");
            }

            try
            {
                config = JFI.GetObject<DistCtl.Config>(configFilename);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                throw new DistException("Configuration file invalid.");
            }

            if (config.EnableLocalConsole)
            {
                prompt = new DistCommon.LivePrompt();
                sayHandler = prompt.Say;
            }
            else
            {
                sayHandler = System.Console.WriteLine;
            }

            controller = new DistCtl.Controller(config, sayHandler);
            controller.Initialize();
        }
    }
}

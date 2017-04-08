namespace DistCtlRuntime
{
    using System;
    using DistCommon;

    internal sealed class Runtime
    {
        DistCtl.Config config;
        Logger.SayHandler sayHandler;
        LivePrompt prompt;
        DistCtl.Controller controller;
        Logger tempLogger;

        public Runtime(string configFilename = DistCommon.Constants.Ctl.ConfigFilename)
        {
            var tempLogger = new Logger(DistCommon.Constants.Ctl.LogFilename);

            string[] dependencies = { configFilename };
            if (new DepMgr(dependencies).FindMissing().Count == 0)
            {
                try
                {
                    config = JFI.GetObject<DistCtl.Config>(configFilename);
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    tempLogger.Log("Configuration file invalid.", 3);
                    Environment.Exit(1);
                }
            }
            else
            {
                tempLogger.Log("Configuration file not found.", 3);
                Environment.Exit(1);
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
        }

        public void Start()
        {
            controller.Initialize();
            if (!this.config.EnableLocalConsole)
            {
                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            }
        }
    }
}

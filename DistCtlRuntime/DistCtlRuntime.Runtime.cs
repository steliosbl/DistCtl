namespace DistCtlRuntime
{
    using System;
    using DistCommon;

    internal sealed class Runtime
    {
        private DistCtl.Config config;
        private Logger.SayHandler sayHandler;
        private LivePrompt prompt;
        private DistCtl.Controller controller;
        private Logger tempLogger;

        public Runtime(string configFilename = DistCommon.Constants.Ctl.ConfigFilename)
        {
            this.tempLogger = new Logger(DistCommon.Constants.Ctl.LogFilename);

            string[] dependencies = { configFilename };
            if (new DepMgr(dependencies).FindMissing().Count == 0)
            {
                try
                {
                    this.config = JFI.GetObject<DistCtl.Config>(configFilename);
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    this.tempLogger.Log("Configuration file invalid.", 3);
                    Environment.Exit(1);
                }
            }
            else
            {
                this.tempLogger.Log("Configuration file not found.", 3);
                Environment.Exit(1);
            }

            if (this.config.EnableLocalConsole)
            {
                this.prompt = new DistCommon.LivePrompt();
                this.sayHandler = this.prompt.Say;
            }
            else
            {
                this.sayHandler = System.Console.WriteLine;
            }

            this.controller = new DistCtl.Controller(this.config, this.sayHandler);
        }

        public void Start()
        {
            try
            {
                this.controller.Initialize();
                if (!this.config.EnableLocalConsole)
                {
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(AggregateException))
                {
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }

                if (!this.config.EnableLiveErrors)
                {
                    this.tempLogger.Log(e.StackTrace, 3);
                    Environment.Exit(1);
                }

                throw;
            }
        }
    }
}

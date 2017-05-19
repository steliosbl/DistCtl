namespace DistCtlRuntime
{
    using System;
    using DistCommon;

    internal sealed class Runtime
    {
        private Config config;
        private Logger.SayHandler sayHandler;
        private DistCtlConsole.Console console;
        private DistCtl.Controller controller;
        private Logger tempLogger;

        public Runtime(string configFilename = DistCommon.Constants.Ctl.ConfigFilename)
        {
            this.tempLogger = new Logger(DistCommon.Constants.Ctl.LogFilename, DistCommon.Constants.CtlRuntime.LoggerSrc);

            string[] dependencies = { configFilename };
            if (new DepMgr(dependencies).FindMissing().Count == 0)
            {
                try
                {
                    this.config = JFI.GetObject<Config>(configFilename);
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
                this.console = new DistCtlConsole.Console();
                this.sayHandler = this.console.Say;
            }
            else
            {
                this.sayHandler = Logger.StdSay;
            }

            this.controller = new DistCtl.Controller(this.config.CtlConfig, this.ExitHandler, this.sayHandler);
            if (this.config.EnableLocalConsole)
            {
                this.console.AddController(this.controller);
            }
        }

        public void Start()
        {
            try
            {
                this.controller.Initialize();
                if (this.config.EnableLocalConsole)
                {
                    new System.Threading.Thread(() => this.console.Start()).Start();
                }

                if (this.config.EnableAPI)
                {
                    new System.Threading.Thread(() => DistCtlApi.API.Run(this.controller)).Start();
                }

                System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
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

        public void ExitHandler()
        {
            this.console.Stop();
            Environment.Exit(0);
        }
    }
}

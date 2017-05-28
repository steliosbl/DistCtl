namespace DistCtlApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public static class API
    {
        public static void Main()
        {
        }

        public static void Run(Config cfg, DistCtl.Controller ctl, DistCommon.Utils.ThreadAwareStreamWriter writer, DistCommon.Logging.Logger.SayHandler sayHandler)
        {
            var logger = new DistCommon.Logging.Logger(DistCommon.Constants.Ctl.LogFilename, DistCommon.Logging.Source.API, sayHandler);

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(services =>
                 {
                     services.AddSingleton<DistCtl.IController>(ctl);
                     services.AddSingleton<DistCommon.Logging.ILogger>(logger);
                 })
                .UseStartup<Startup>()
                .UseUrls(string.Format("http://*:{0}", cfg.Port.ToString()))
                .Build();

            if (cfg.Enable)
            {
                new System.Threading.Thread(o =>
                {
                    DistCommon.Utils.ThreadAwareStreamWriter threadWriter = (DistCommon.Utils.ThreadAwareStreamWriter)o;
                    threadWriter.RegisterThreadWriter(new DistCommon.Logging.LogWriter(logger));
                    host.Run();
                }).Start(writer);
            }
        }
    }
}

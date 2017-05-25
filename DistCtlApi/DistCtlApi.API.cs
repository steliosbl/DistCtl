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

        public static void Run(Config cfg, DistCtl.Controller ctl)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(services =>
                 {
                     services.AddSingleton<DistCtl.IController>(ctl);
                     services.AddSingleton<DistCommon.Logging.ILogger>(new DistCommon.Logging.Logger(DistCommon.Constants.Ctl.LogFilename, DistCommon.Logging.Source.API));
                 })
                .UseStartup<Startup>()
                .UseUrls(string.Format("http://*:{0}", cfg.Port.ToString()))
                .Build();

            if (cfg.Enable)
            {
                host.Run();
            }
        }
    }
}

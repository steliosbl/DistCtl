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

    public class API
    {
        ////public static void Main(string[] args)
        ////{
        ////    var host = new WebHostBuilder()
        ////        .UseKestrel()
        ////        .UseContentRoot(Directory.GetCurrentDirectory())
        ////        .UseIISIntegration()
        ////        .UseStartup<Startup>()
        ////        .Build();

        ////    host.Run();
        ////}

        public static void Run(DistCtl.Controller ctl)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(services => services.AddSingleton<DistCtl.IController>(ctl))
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

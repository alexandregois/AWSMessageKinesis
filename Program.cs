using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace OFD_MessageService
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = CreateHostBuilder(args);

            // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            //     builder.UseWindowsService();
            //     builder.ConfigureLogging((_, logging) => logging.AddEventLog());
            // }

            builder.Build().Run();
            //var host = builder.Build();
            //host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) => 
                services.AddHostedService<KinesisWorker>()
            );
    }
}

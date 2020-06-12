using System;
using Coravel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AWS.MessageService.KINESIS
{
    public class Program
    {
        private static IConfiguration configuration { get; }
        public static int tempoExec { get; set; }

        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            host.Services.UseScheduler(scheduler =>
            {
                //scheduler.Schedule<MessageInvocable>().EveryFiveMinutes();//.EveryFiveMinutes();
                scheduler.Schedule<MessageInvocable>().EveryFifteenSeconds();

            });
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureServices(services =>
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())  //location of the exe file
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            tempoExec = Convert.ToInt32(configuration.GetSection("MySettings").GetSection("TempoExec").Value);

            services.AddScheduler();
            services.AddTransient<MessageInvocable>();
        });
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OFD_SendMail_Worker.API;

namespace OFD_MessageService
{
    public class KinesisWorker : BackgroundService
    {
        private readonly ILogger<KinesisWorker> logger; 
        private readonly IConfiguration config;
        private readonly IConfigurationSection workConfig;

        private HttpClient hClient = new HttpClient();

        public KinesisWorker(ILogger<KinesisWorker> iLogger, IConfiguration iConfig)
        {
            logger = iLogger;
            config = iConfig;
            workConfig = config.GetSection("MySettings");

            logger.LogInformation($"KinesisWorker criado em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ...");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"KinesisWorker em execução em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ...");

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //logger.LogInformation($"KinesisWorker iniciando tarefa em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ...");

            ServiceReader.Execute(workConfig, stoppingToken, logger);

                //logger.LogInformation($"KinesisWorker concluído tarefa em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss}  ...");
            //}

            logger.LogInformation($"KinesisWorker terminando em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ...");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("KinesisWorker iniciando ...");
    
            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("KinesisWorker terminando ...");
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            logger.LogInformation("KinesisWorker dispensando recursos .........");
            base.Dispose();
        }

        private void LogResult(string text, bool isOk)
        {
            if (isOk)
                logger.LogInformation($"KinesisWorker Sucesso ao {text}");
            else
                logger.LogError($"KinesisWorker Falha ao {text}");
        }     
    }
}

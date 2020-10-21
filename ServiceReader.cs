using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using Amazon.Kinesis.Model;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using RepoDb;
using RepoDb.Extensions;
using System.Data;
using System.Globalization;
using System.Data.SqlClient;
using OFD_MessageService.Moc;
using OFD_MessageService.Dto;
using OFD_MessageService.Enum;
using Serilog;

namespace OFD_MessageService
{
    public class ServiceReader
    {
        //private static readonly AmazonKinesisClient kinesisClient = new AmazonKinesisClient(RegionEndpoint.EUWest2);

        private static AmazonKinesisClient _client;
        private static IConfigurationSection workConfig;
        private static ILogger<KinesisWorker> logger;

        public static string nomeTagOld { get; set; }
        public static string nomeTagAtual { get; set; }
        public static string prefixoTagOld { get; set; }
        public static string prefixoTagAtual { get; set; }

        public static Dictionary<string, string> dicPrefixo { get; set; }

        public static string mensagem1, mensagem2;

        public string iteratorId { get; set; }
        private static IConfiguration configuration { get; }
        public static bool isRunning { get; set; }
        public static int tempoExec { get; set; }



        public static string IP = "208.115.211.92";
        //public static string DB = "ofduoblpdb002";
        public static string DB = "OFDUOBLPDB007";


        public static string Pwd()
        {
            return ASCIIEncoding.ASCII.GetString(Convert.FromBase64String("xxxxxxxx"));
        }

        public static CancellationToken stoppingToken;

        public static void Execute(IConfigurationSection config, CancellationToken pStoppingToken, ILogger<KinesisWorker> pLogger)
        {

            RepoDb.SqlServerBootstrap.Initialize();

            if (!isRunning)
                isRunning = true;
            else
                return;

            workConfig = config;
            stoppingToken = pStoppingToken;
            logger = pLogger;


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(workConfig["LogPath"], rollingInterval: RollingInterval.Day)
                .CreateLogger();


            GravaLog.workConfig = workConfig;


            logger.LogInformation($"KinesisWorker.ServiceReader preparando execução em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ...");

            //GravaLog.GravaLogInformacao($"KinesisWorker.ServiceReader preparando execução em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ...");

            ReadFromKinesis().Wait();
        }

        private static Gaiola RetornaGaiola(string pPrefixo)
        {
            //int retorno = 0;

            Gaiola retorno = new Gaiola();

            try
            {
                if (pPrefixo.IndexOf("HF113") > -1)
                {

                    if (dicPrefixo.TryGetValue(pPrefixo + ".TB_Z8", out string valueBD1))
                        if (Convert.ToInt32(valueBD1) > 0)
                            retorno.Num_Bloco = Convert.ToInt32(valueBD1);


                    if (retorno.Num_Bloco > 0)
                    {
                        retorno.Id = (int)eGaiola.BD1;
                        retorno.Tipo = "BD1";
                    }

                }

                if (pPrefixo.IndexOf("HH113") > -1)
                {
                    if (dicPrefixo.TryGetValue(pPrefixo + ".TB_Z13", out string valueBD2))
                        if (Convert.ToInt32(valueBD2) > 0)
                            retorno.Num_Bloco = Convert.ToInt32(valueBD2);


                    if (retorno.Num_Bloco > 0)
                    {
                        retorno.Id = (int)eGaiola.BD2;
                        retorno.Tipo = "BD2";
                    }
                }


                if (pPrefixo.IndexOf("JN112") > -1)
                {

                    if (dicPrefixo.TryGetValue(pPrefixo + ".TB_Z19", out string valueTANDEM))
                        if (Convert.ToInt32(valueTANDEM) > 0)
                            retorno.Num_Bloco = Convert.ToInt32(valueTANDEM);

                    if (retorno.Num_Bloco > 0)
                    {
                        if (dicPrefixo.TryGetValue(pPrefixo + ".CS_HRT2HD", out string valueUR2))
                        {
                            if (valueUR2 != null)
                            {
                                retorno.Id = (int)eGaiola.UR2;
                                retorno.Tipo = "UR2";
                            }
                        }

                        if (dicPrefixo.TryGetValue(pPrefixo + ".CS_HRTEHD", out string valueE3))
                        {
                            if (valueE3 != null)
                            {
                                retorno.Id = (int)eGaiola.EDGE;
                                retorno.Tipo = "EDGE";
                            }
                        }

                        if (dicPrefixo.TryGetValue(pPrefixo + ".CS_HRTNHD", out string valueUR2N))
                        {
                            if (valueUR2N != null)
                            {
                                retorno.Id = (int)eGaiola.UR2N;
                                retorno.Tipo = "UR2N";
                            }
                        }

                        if (dicPrefixo.TryGetValue(pPrefixo + ".CC_HRT2_1", out string valueC2))
                        {
                            if (valueC2 != null)
                            {
                                retorno.Id = (int)eGaiola.C2;
                                retorno.Tipo = "C2";
                            }
                        }

                        if (dicPrefixo.TryGetValue(pPrefixo + ".CC_HRTE_1", out string valueC2N))
                        {
                            if (valueC2N != null)
                            {
                                retorno.Id = (int)eGaiola.C2N;
                                retorno.Tipo = "C2N";
                            }
                        }
                    }


                }

            }
            catch (Exception ex)
            {

                GravaLog.GravaLogErro(ex.Message);
            }

            return retorno;
        }

        public static async Task ReadFromKinesis()
        {
            dicPrefixo = new Dictionary<string, string>();
            var file = "last_iterator_id.txt";
            _client = new AmazonKinesisClient(workConfig["AccessKeyAWS"], workConfig["SecretKeyAWS"], RegionEndpoint.USEast1);

            try
            {
                var describer = await _client.DescribeStreamAsync(new DescribeStreamRequest { StreamName = workConfig["QueueNamedAWS"] });
                var shards = describer.StreamDescription.Shards;

                foreach (var shard in shards)
                {
                    var iterator_request = new GetShardIteratorRequest
                    {
                        StreamName = workConfig["QueueNamedAWS"],
                        ShardId = shard.ShardId,
                        ShardIteratorType = ShardIteratorType.LATEST,
                        Timestamp = DateTime.MinValue
                    };

                    var iterator = await _client.GetShardIteratorAsync(iterator_request);
                    string curr_iterator_id = iterator.ShardIterator;
                    string last_iterator_id = File.Exists(file) ? File.ReadAllText(file) : string.Empty;

                    if (!string.IsNullOrEmpty(last_iterator_id))
                        curr_iterator_id = last_iterator_id;

                    while (!string.IsNullOrEmpty(curr_iterator_id))
                    {
                        var response = await _client.GetRecordsAsync(new GetRecordsRequest { ShardIterator = curr_iterator_id, Limit = 1 });
                        var next_iterator_id = response.NextShardIterator;
                        var records = response.Records;

                        //Console.Write("\r\n" + "Sequencial: " + curr_iterator_id + "\r\n");

                        if (records.Count > 0)
                        {
                            foreach (var record in records)
                            {
                                var strData = Encoding.UTF8.GetString(record.Data.ToArray());
                                var words = strData.Split(new string[] { "\\n" }, StringSplitOptions.None);

                                foreach (var item in words)
                                {

                                    Console.Write(item);

                                    var lines = item.Split('\n');

                                    for (int i = 0; i < lines.Count() - 1; i++)
                                    {

                                        //GravaLog.GravaLogInformacao("Split linha registro - ServiceReader Linha: 194");

                                        var msg_partes = lines[i].Split(',');
                                        //GravaLog.GravaLogInformacao("msg_partes - ServiceReader Linha: 197");

                                        var addressAndTag = msg_partes[0].Replace("ns=2;s=", "");
                                        //GravaLog.GravaLogInformacao("addressAndTag - ServiceReader Linha: 200");

                                        string prefixoTagAtual;

                                        //como o codigo para ler o numero fila do tipo 1 foi separado do tipo 2 então agora o separador é constante.
                                        char separator = '.'; //Configuracoes.NumeroFila == "1" ? '\\' : '.';

                                        if (addressAndTag.IndexOf(separator) == -1)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            var tagPartes = addressAndTag.Split(separator);
                                            //GravaLog.GravaLogInformacao("tagPartes - ServiceReader Linha: 214");

                                            var tagPartesPrefixo = tagPartes[0].Split(separator);
                                            //GravaLog.GravaLogInformacao("tagPartesPrefixo - ServiceReader Linha: 217");

                                            prefixoTagAtual = tagPartes[3];
                                            //GravaLog.GravaLogInformacao("prefixoTagAtual - ServiceReader Linha: 220");

                                            nomeTagAtual = tagPartes[4].Split('\\').LastOrDefault();
                                            //GravaLog.GravaLogInformacao("nomeTagAtual - ServiceReader Linha: 223");
                                        }

                                        string strTagCombinada = prefixoTagAtual + "." + nomeTagAtual;


                                        CultureInfo culture = new CultureInfo("en-US");
                                        DateTime dtRegistro;
                                        //GravaLog.GravaLogInformacao("dtRegistro - ServiceReader Linha: 231");
                                        dtRegistro = Convert.ToDateTime(msg_partes[2].TrimEnd(), new CultureInfo("en-US"));


                                        //Grava Registro
                                        //GravaLog.GravaLogInformacao("Data.SetFila_Tabela - Grava fila completa - ServiceReader Linha: 236");
                                        Data.SetFila_Tabela(Conexao.GetConnectionHist(workConfig), lines[i], dtRegistro, workConfig["QueueNamedAWS"],
                                            nomeTagAtual, msg_partes[1].TrimEnd(), prefixoTagAtual, curr_iterator_id);


                                        Int32 id_t_bitola = 0;
                                        GravaLog.GravaLogInformacao("Verifica se há nome da bitola no dicionario - ServiceReader Linha: 242");
                                        if (dicPrefixo.TryGetValue(prefixoTagAtual + ".SP_BITOLA", out string valueTagBitola))
                                        {
                                            GravaLog.GravaLogInformacao("Retorna nome da bitola - ServiceReader Linha: 245");
                                            id_t_bitola = Data.GetIdBitola(Conexao.GetConnectionProd(workConfig), valueTagBitola);
                                        }


                                        //Grava na Tabela de Mudança
                                        if (dicPrefixo.TryGetValue(strTagCombinada, out string valueTag))
                                        {
                                            GravaLog.GravaLogInformacao("Verifica mudança de valores - ServiceReader Linha: 253");
                                            if (valueTag != null && valueTag != String.Empty)
                                            {
                                                GravaLog.GravaLogInformacao("Troca o valor da Tag no dicionario - ServiceReader Linha: 256");
                                                dicPrefixo.Remove(strTagCombinada);
                                                dicPrefixo.Add(strTagCombinada, msg_partes[1].TrimEnd());

                                                if (msg_partes[1].TrimEnd() != valueTag)
                                                {

                                                    GravaLog.GravaLogInformacao("Data.SetFila_Tabela_Mudanca - Grava mudanca na tabela - Tag: " + nomeTagAtual +
                                                    " Valor Anterior: " + valueTag + " Valor Atual: " + msg_partes[1].TrimEnd() + " - ServiceReader Linha: 263");

                                                    Data.SetFila_Tabela_Mudanca(Conexao.GetConnectionHist(workConfig), workConfig["QueueNamedAWS"], nomeTagAtual,
                                                        msg_partes[1].TrimEnd(), prefixoTagAtual, valueTag);


                                                    GravaLog.GravaLogInformacao("Identificada Tag que retorna posicao do canal - ServiceReader Linha: 268");

                                                    if (nomeTagAtual == "GRVATUAL" || nomeTagAtual == "CC_CURPGVNB")
                                                    {

                                                        if (dicPrefixo.TryGetValue(prefixoTagAtual + ".SS_BD1OUT", out string valueTagBD1OUT))
                                                        {
                                                            GravaLog.GravaLogInformacao("BD1 desligada - ServiceReader Linha: 276");
                                                            if (valueTagBD1OUT == "1")
                                                                continue;
                                                        }


                                                        GravaLog.GravaLogInformacao("Retorna ID da gaiola - ServiceReader Linha: 281");

                                                        Gaiola gaiola = null;

                                                        if (prefixoTagAtual == "HH113")
                                                        {
                                                            if (dicPrefixo.TryGetValue(prefixoTagAtual + ".TB_Z13", out string valueBD2))
                                                                if (Convert.ToInt32(valueBD2) == 0)
                                                                    gaiola = RetornaGaiola("HF113");
                                                                else
                                                                    gaiola = RetornaGaiola(prefixoTagAtual);

                                                        }
                                                        else
                                                            gaiola = RetornaGaiola(prefixoTagAtual);


                                                        if (gaiola != null && gaiola.Num_Bloco > 0)
                                                        {
                                                            id_t_bitola = Data.GetIdBitolaNumBloco(Conexao.GetConnectionProd(workConfig), gaiola.Num_Bloco);

                                                            if (id_t_bitola > 0)
                                                            {
                                                                GravaLog.GravaLogInformacao("Data.SetLaminacaoCambio_Canal - Grava dados do canal - " + "ID Bitola: - " + id_t_bitola + " ID Gaiola: " + gaiola.Id +
                                                                    " Posicao: " + Convert.ToInt32(msg_partes[1].TrimEnd()) + " - ServiceReader Linha: 288");
                                                                Data.SetLaminacaoCambio_Canal(Conexao.GetConnectionProd(workConfig), id_t_bitola, Convert.ToInt32(msg_partes[1].TrimEnd()), gaiola.Id, gaiola.Num_Bloco);
                                                            }
                                                        }

                                                    }

                                                }
                                            }
                                        }
                                        else
                                        {

                                            dicPrefixo.Add(strTagCombinada, msg_partes[1].TrimEnd());

                                            GravaLog.GravaLogInformacao("Identificada Tag que retorna posicao do canal - dicionario limpo - ServiceReader Linha: 320");

                                            if (nomeTagAtual == "GRVATUAL" || nomeTagAtual == "CC_CURPGVNB")
                                            {

                                                if (dicPrefixo.TryGetValue(prefixoTagAtual + ".SS_BD1OUT", out string valueTagBD1OUT))
                                                {
                                                    if (valueTagBD1OUT == "1")
                                                        continue;
                                                }


                                                Gaiola gaiola = null;

                                                if (prefixoTagAtual == "HH113")
                                                {
                                                    if (dicPrefixo.TryGetValue(prefixoTagAtual + ".TB_Z13", out string valueBD2))
                                                        if (Convert.ToInt32(valueBD2) == 0)
                                                            gaiola = RetornaGaiola("HF113");
                                                        else
                                                            gaiola = RetornaGaiola(prefixoTagAtual);
                                                }
                                                else
                                                    gaiola = RetornaGaiola(prefixoTagAtual);


                                                if (gaiola != null && gaiola.Num_Bloco > 0)
                                                {
                                                    id_t_bitola = Data.GetIdBitolaNumBloco(Conexao.GetConnectionProd(workConfig), gaiola.Num_Bloco);

                                                    if (id_t_bitola > 0)
                                                    {
                                                        GravaLog.GravaLogInformacao("Data.SetLaminacaoCambio_Canal - Grava dados do canal - " + "ID Bitola: - " + id_t_bitola + " ID Gaiola: " + gaiola.Id +
                                                                                  " Posicao: " + Convert.ToInt32(msg_partes[1].TrimEnd()) + " - ServiceReader Linha: 319");
                                                        Data.SetLaminacaoCambio_Canal(Conexao.GetConnectionProd(workConfig), id_t_bitola, Convert.ToInt32(msg_partes[1].TrimEnd()), gaiola.Id, gaiola.Num_Bloco);

                                                    }
                                                }

                                            }

                                        }

                                    }

                                }
                            }

                        }

                        //GravaLog.GravaLogInformacao("Novo registro da fila");
                        File.WriteAllText(file, next_iterator_id);
                        curr_iterator_id = next_iterator_id;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"KinesisWorker.ServiceReader exceção em {DateTimeOffset.Now:dd/MM/yyy HH:mm:ss} ... {ex.Message}");

                GravaLog.GravaLogErro(ex.Message);

                if (!stoppingToken.IsCancellationRequested)
                    await ReadFromKinesis();
            }
            finally
            {
                if (!stoppingToken.IsCancellationRequested)
                    await ReadFromKinesis();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Kinesis;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Coravel.Invocable;
using Microsoft.Extensions.Configuration;
using Amazon.Kinesis.Model;
using AWS.MessageService.KINESIS.Dominio;
using Newtonsoft.Json;

namespace AWS.MessageService.KINESIS
{
    public class MessageInvocable : IInvocable
    {
        // ***** GERDAU ***********************

        /*
        private static string urlAWS = "https://sqs.us-east-1.amazonaws.com/";  //066105611759/ows_lam3";
        private static string userIdAWS = "066105611759";
        private static string queueNamedAWS = "ows_lam3";
        //private static string queueNamedAWS = "ows_lam3_robo";
        private static string accessKeyAWS = "AKIAQ6ZBTSXXZ7GPNBON";
        private static string secretKeyAWS = "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";
        */
        private static string urlAWS;
        private static string userIdAWS;
        private static string queueNamedAWS;
        private static string accessKeyAWS;
        private static string secretKeyAWS;
        private static string databaseIp;
        private static string databaseName;
        private static string tableName;
        private static string userDb;
        private static string passDb;

        private static IConfiguration configuration;


        private static readonly AmazonKinesisClient kinesisClient =
            new AmazonKinesisClient(RegionEndpoint.EUWest2);


        const string myStreamName = "myTestStream";

        private static AmazonKinesisClient _client;
        private static string _streamName;

        private SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();


        public Task Invoke()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())  //location of the exe file
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            urlAWS = configuration.GetSection("MySettings").GetSection("UrlAWS").Value;
            userIdAWS = configuration.GetSection("MySettings").GetSection("UserIdAWS").Value;
            queueNamedAWS = configuration.GetSection("MySettings").GetSection("QueueNamedAWS").Value;
            accessKeyAWS = configuration.GetSection("MySettings").GetSection("AccessKeyAWS").Value;
            secretKeyAWS = configuration.GetSection("MySettings").GetSection("SecretKeyAWS").Value;

            databaseIp = configuration.GetSection("MySettings").GetSection("DatabaseIp").Value;
            databaseName = configuration.GetSection("MySettings").GetSection("DatabaseName").Value;
            tableName = configuration.GetSection("MySettings").GetSection("TableName").Value;
            userDb = configuration.GetSection("MySettings").GetSection("UserDb").Value;
            passDb = configuration.GetSection("MySettings").GetSection("PassDb").Value;


            //DataBase
            sqlBuilder = new SqlConnectionStringBuilder();

            sqlBuilder.DataSource = databaseIp;
            sqlBuilder.UserID = userDb;
            sqlBuilder.Password = passDb;
            sqlBuilder.InitialCatalog = databaseName;
            //


            //_client = new AmazonKinesisClient(accessKeyAWS, secretKeyAWS, RegionEndpoint.USEast1);
            //_streamName = queueNamedAWS;

            _client = new AmazonKinesisClient("AKIA445VGZTO7BIS2LRJ", "0m+DlUtuRF0xe1pyRTS7WCmqNVlhCoJ/kHpbzKsR", RegionEndpoint.USEast1);
            _streamName = "UOB_LPE";


            //ReadQueueAWS();

            ReadFromKinesis();


            Console.WriteLine("Leitura realizada.");
            return Task.CompletedTask;
        }

        private void ReadQueueAWS()
        {

            try
            {

                var queueUrl = urlAWS + userIdAWS + "/" + queueNamedAWS;

                Console.WriteLine("Queue Test Starting!");
                Console.WriteLine("Creating Client and request");

                //Create some Credentials with our IAM user
                var awsCreds = new BasicAWSCredentials(accessKeyAWS, secretKeyAWS);

                //Create a client to talk to SQS
                var amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.USEast1); //Gerdau
                Console.WriteLine("Receiving Message");

                //Create a receive requesdt to see if there are any messages on the queue
                var receiveMessageRequest = new ReceiveMessageRequest();
                //receiveMessageRequest.MaxNumberOfMessages = 10;
                //receiveMessageRequest.VisibilityTimeout = (int)TimeSpan.FromMinutes(1).TotalSeconds;
                //receiveMessageRequest.WaitTimeSeconds = (int)TimeSpan.FromSeconds(30).TotalSeconds;
                receiveMessageRequest.QueueUrl = queueUrl;


                //Send the receive request and wait for the response
                var response = amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest).Result;

                //If we have any messages available
                if (response.Messages.Any())
                {

                    List<Peca> listaPecas = new List<Peca>();

                    List<String> listItensQuery = new List<string>();


                    foreach (var message in response.Messages)
                    {
                        //Spit it out
                        Console.WriteLine(message.Body);

                        string[] words = message.Body.Split(new string[] { "\\n" }, StringSplitOptions.None);
                        string[] lines = words[0].Split('\n');

                        //foreach (var item in lines)
                        //{

                        for (int i = 0; i < lines.Count() - 1; i++)
                        {

                            string[] partes = lines[i].Split(',');

                            string strTag = partes[0].Replace("ns=2;s=", "");

                            string[] tagPartes = null;

                            string strTagPartesDB;

                            if (queueNamedAWS != "ows_lam3")
                            {
                                tagPartes = strTag.Split('\\');
                                strTagPartesDB = tagPartes[2];
                            }
                            else
                            {
                                tagPartes = strTag.Split('.');
                                strTagPartesDB = tagPartes[2];
                            }



                            CultureInfo culture = new CultureInfo("en-US");
                            DateTime dtRegistro = Convert.ToDateTime(partes[2], culture);


                            using (SqlConnection connection = new SqlConnection(sqlBuilder.ConnectionString))
                            {

                                try
                                {

                                    connection.Open();
                                    using (SqlCommand command = connection.CreateCommand())
                                    {
                                        /*
                                         
                                        command.CommandText = "INSERT INTO " + tableName + "([LinhaFila],[Datafila],[DataProcessamento],[NomeFila],[Tag],[Valor],[MessageID],[Tag1])" +
                                        " VALUES(@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";

                                        command.Parameters.AddWithValue("@param1", lines[i]);
                                        command.Parameters.AddWithValue("@param2", dtRegistro);
                                        command.Parameters.AddWithValue("@param3", DateTime.Now);
                                        command.Parameters.AddWithValue("@param4", queueNamedAWS);
                                        command.Parameters.AddWithValue("@param5", strTag);
                                        command.Parameters.AddWithValue("@param6", partes[1].ToString());
                                        command.Parameters.AddWithValue("@param7", message.MessageId);
                                        command.Parameters.AddWithValue("@param8", strTagPartesDB);

                                        */


                                        command.CommandText = "INSERT INTO " + tableName +
                                            "([conteudo_linha],[conteudo_data],[conteudo_nome_fila],[conteudo_nome_tag]," +
                                            "[conteudo_tag_valor],[conteudo_message_id],[data_processamento])" +
                                        " VALUES(@param1, @param2, @param3, @param4, @param5, @param6, @param7)";


                                        command.Parameters.AddWithValue("@param1", lines[i]);
                                        command.Parameters.AddWithValue("@param2", dtRegistro);
                                        command.Parameters.AddWithValue("@param3", queueNamedAWS);
                                        command.Parameters.AddWithValue("@param4", strTagPartesDB);
                                        command.Parameters.AddWithValue("@param5", partes[1].ToString());
                                        command.Parameters.AddWithValue("@param6", message.MessageId);
                                        command.Parameters.AddWithValue("@param7", DateTime.Now);


                                        command.ExecuteNonQuery();

                                        connection.Close();

                                    }

                                }
                                catch (SqlException ex)
                                {
                                    connection.Close();
                                }


                            }
                        }


                        //Remove it from the queue as we don't want to see it again

                        var deleteMessageRequest = new DeleteMessageRequest();
                        deleteMessageRequest.QueueUrl = queueUrl;
                        deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                        var result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;

                    }

                }

            }
            catch (Exception ex)
            {

            }
        }


        private async Task ReadFromKinesis()
        {
            try
            {

                var kinesisStreamName = _streamName;

                var describeRequest = new DescribeStreamRequest
                {
                    StreamName = kinesisStreamName,
                };

                var describeResponse = await _client.DescribeStreamAsync(describeRequest);
                var shards = describeResponse.StreamDescription.Shards;

                foreach (var shard in shards)
                {
                    var iteratorRequest = new GetShardIteratorRequest
                    {
                        StreamName = kinesisStreamName,
                        ShardId = shard.ShardId,
                        ShardIteratorType = ShardIteratorType.AT_TIMESTAMP,
                        //ShardIteratorType = ShardIteratorType.AT_SEQUENCE_NUMBER,
                        //StartingSequenceNumber = lastSeenSequenceNumber,
                        Timestamp = DateTime.MinValue
                    };

                    var iteratorResponse = await _client.GetShardIteratorAsync(iteratorRequest);
                    var iteratorId = iteratorResponse.ShardIterator;

                    while (!string.IsNullOrEmpty(iteratorId))
                    {
                        var getRequest = new GetRecordsRequest
                        {
                            ShardIterator = iteratorId,
                            Limit = 10000
                        };

                        var getResponse = await _client.GetRecordsAsync(getRequest);
                        var nextIterator = getResponse.NextShardIterator;
                        var records = getResponse.Records;

                        if (records.Count > 0)
                        {
                            Console.WriteLine("Received {0} records. ", records.Count);
                            foreach (var record in records)
                            {
                                var json = Encoding.UTF8.GetString(record.Data.ToArray());
                                //Console.WriteLine("Json string: " + json);
                                //Console.WriteLine("====================================================================");
                                //Console.WriteLine(json);

                                string[] words = json.Split(new string[] { "\\n" }, StringSplitOptions.None);
                                string[] lines = words[0].Split('\n');



                                for (int i = 0; i < lines.Count() - 1; i++)
                                {

                                    string[] partes = lines[i].Split(',');

                                    string strTag = partes[0].Replace("ns=2;s=", "");

                                    string[] tagPartes = null;

                                    string strTagPartesDB;

                                    if (queueNamedAWS != "ows_lam3")
                                    {
                                        tagPartes = strTag.Split('\\');
                                        strTagPartesDB = tagPartes[2];
                                    }
                                    else
                                    {
                                        tagPartes = strTag.Split('.');
                                        strTagPartesDB = tagPartes[2];
                                    }


                                    CultureInfo culture = new CultureInfo("en-US");
                                    DateTime dtRegistro = Convert.ToDateTime(partes[2], culture);


                                    using (SqlConnection connection = new SqlConnection(sqlBuilder.ConnectionString))
                                    {

                                        try
                                        {

                                            connection.Open();
                                            using (SqlCommand command = connection.CreateCommand())
                                            {

                                                //command.CommandText = "INSERT INTO " + tableName +
                                                //    "([conteudo_linha],[conteudo_data],[conteudo_nome_fila],[conteudo_nome_tag]," +
                                                //    "[conteudo_tag_valor],[conteudo_message_id],[data_processamento])" +
                                                //" VALUES(@param1, @param2, @param3, @param4, @param5, @param6, @param7)";

                                                command.CommandText = "INSERT INTO " + tableName +
                                                    "([conteudo_linha],[conteudo_data],[conteudo_nome_fila],[conteudo_nome_tag]," +
                                                    "[conteudo_tag_valor],[conteudo_message_id])" +
                                                " VALUES(@param1, @param2, @param3, @param4, @param5, @param6)";

                                                command.Parameters.AddWithValue("@param1", lines[i]);
                                                command.Parameters.AddWithValue("@param2", dtRegistro);
                                                command.Parameters.AddWithValue("@param3", queueNamedAWS);
                                                command.Parameters.AddWithValue("@param4", strTagPartesDB);
                                                command.Parameters.AddWithValue("@param5", partes[1].ToString());
                                                command.Parameters.AddWithValue("@param6", iteratorId);
                                                //command.Parameters.AddWithValue("@param7", DateTime.Now);


                                                command.ExecuteNonQuery();

                                                connection.Close();

                                                //Console.WriteLine(lines[i]);
                                                Console.WriteLine("Registro gravado");

                                            }

                                        }
                                        catch (SqlException ex)
                                        {
                                            Console.WriteLine(ex.Message);

                                            connection.Close();
                                        }
                                        finally
                                        {

                                        }


                                    }

                                }




                            }
                        }

                        
                        iteratorId = nextIterator;

                        ////Remove it from the queue as we don't want to see it again
                        //var deleteMessageRequest = new DeleteMessageRequest();
                        //deleteMessageRequest.QueueUrl = queueUrl;
                        //deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;
                        //var result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

    }
}

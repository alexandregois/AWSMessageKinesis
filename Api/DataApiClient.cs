using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

#pragma warning disable CS1701 // Assuming assembly reference matches identity

namespace OFD_SendMail_Worker.API
{
    public class DataApiClient<T> 
    {
        StoredProcedureResponse<T> objData = new StoredProcedureResponse<T>();

        private HttpClient client;
        public HttpClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new HttpClient(new HttpClientHandler
                    {
                        AllowAutoRedirect = false,
                        UseCookies = true
                    });

                    client.Timeout = TimeSpan.FromSeconds(70);
                }

                return client;
            }
        }

        public async Task<StoredProcedureResponse<T>> GetBy(string sUrl, Object param, bool ignoreNull = true)
        {
            try
            {
                var netGuid = Guid.NewGuid();
                var data = string.Empty;
                var json = string.Empty;

                if (param != null)
                {
                    if (param.GetType().IsConstructedGenericType && param.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        json = JsonConvert.SerializeObject(param, Formatting.Indented);
                    }
                    else
                    {
                        if (ignoreNull)
                            json = JsonConvert.SerializeObject(param, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        else
                            json = JsonConvert.SerializeObject(param, Formatting.None);
                    }
                }

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Debug.WriteLine($"{netGuid} -> {sUrl} - {json}");

                var resp = await this.Client.PostAsync(sUrl, content);
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    data = await resp.Content.ReadAsStringAsync();
                    objData = JsonConvert.DeserializeObject<StoredProcedureResponse<T>>(data);
                }

                if (objData?.ResultSets?.Count > 0)
                    Debug.WriteLine( $"{netGuid} <- {resp.StatusCode} - [{objData?.ResultSets?[0]?.Count}] iten(s) - {data}");
                else
                    Debug.WriteLine($"{netGuid} <- {resp.StatusCode} - ret='{objData?.ReturnValue?.ToString()}' - {data}");
            }
            catch (System.Net.Http.HttpRequestException) { }
            catch (System.Net.WebException) { }
            catch (System.Exception) { }

            return objData;
        }


        public async Task<int> Save(string sUrl, T param)
        {
            int iRet = 0;
            string data = string.Empty;
            string json = string.Empty;

            if (param == null)
                return 0;

            if (param.GetType().IsConstructedGenericType && param.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                json = JsonConvert.SerializeObject(param, Formatting.Indented);
            else
                json = JsonConvert.SerializeObject(param, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            try
            {
                var netGuid = Guid.NewGuid();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Debug.WriteLine($"{netGuid} -> {sUrl} - {json}");

                var resp = this.Client.PostAsync(sUrl, content).Result;
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    data = await resp.Content.ReadAsStringAsync();
                    objData = JsonConvert.DeserializeObject<StoredProcedureResponse<T>>(data);
                }

                if (objData?.ResultSets?.Count > 0)
                    Debug.WriteLine($"{netGuid} <- {resp.StatusCode} - [{objData?.ResultSets?[0]?.Count}] iten(s) - {data}");
                else
                    Debug.WriteLine($"{netGuid} <- {resp.StatusCode} - ret='{objData?.ReturnValue?.ToString()}' - {data}");

            }
            catch (System.Net.Http.HttpRequestException ) {  }
            catch (System.Net.WebException ) { }
            catch (System.Exception) { }

            if (objData != null && objData.ReturnValue != null)
                int.TryParse(objData.ReturnValue.ToString(), out iRet);

            return iRet;
        }

        public async Task<int> SaveObject(string sUrl, Object param, bool debug_calls = false)
        {
            int iRet = 0;

            if (param == null)
                return 0;

            try
            {
                var data = string.Empty;
                var json = string.Empty;
                var netGuid = Guid.NewGuid();

                if (param.GetType().IsConstructedGenericType && param.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    json = JsonConvert.SerializeObject(param, Formatting.Indented);
                else
                    json = JsonConvert.SerializeObject(param, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Debug.WriteLine($"{netGuid} -> {sUrl} - {json}");

                if (debug_calls)
                    Console.WriteLine($"Tx ===> {netGuid} -> {sUrl} - {json}");

                var resp = this.Client.PostAsync(sUrl, content).Result;
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    data = await resp.Content.ReadAsStringAsync();
                    objData = JsonConvert.DeserializeObject<StoredProcedureResponse<T>>(data);
                }

                //if (debug_calls)
                    //Console.WriteLine($"RX <=== {data}");

                if (debug_calls)
                { 
                    if (objData?.ResultSets?.Count > 0)
                        Console.WriteLine($"Rx <=== {netGuid} <- {resp.StatusCode} - [{objData?.ResultSets?[0]?.Count}] iten(s) - {data}");
                    else
                        Console.WriteLine($"Rx <=== {netGuid} <- {resp.StatusCode} - ret='{objData?.ReturnValue?.ToString()}' - {data}");
                }
            }
            catch (HttpRequestException hex) {
                if (debug_calls) Console.WriteLine($"ERROR === {hex.Message}");
            }
            catch (System.Net.WebException wex) {
                if (debug_calls) Console.WriteLine($"ERROR === {wex.Message}");
            }
            catch (Exception ex) {
                if (debug_calls) Console.WriteLine($"ERROR === {ex.Message}");
            }

            if (objData != null && objData.ReturnValue != null)
                int.TryParse(objData.ReturnValue.ToString(), out iRet);

            return iRet;
        }
    }
}

#pragma warning restore CS1701 // Assuming assembly reference matches identity

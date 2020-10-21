using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OFD_SendMail_Worker.API
{
    public class Api
    {
        public async Task<bool> SetDbObject(string hostAddr, string database, string procedure, Object obj, bool debug_calls = false)
        {
            string _Url = $"http://{hostAddr}/{database}.dbo.{procedure}/json";
            if (string.IsNullOrEmpty(procedure) || obj == null)
                return false;

            return await new DataApiClient<Object>().SaveObject(_Url, obj, debug_calls).ContinueWith((x) =>
            {
                if (x.Status == TaskStatus.RanToCompletion)
                    return (x.Result > 0);
                else
                    return false;
            });
        }


        public async Task<int> SetInt<T>(string hostAddr, string database, string procedure, T obj, bool showLoading = false)
        {
            string _URL = $"http://{hostAddr}/{database}.dbo.{procedure}/json";

            if (string.IsNullOrEmpty(procedure) || obj == null)
                return 0;

            var client = new DataApiClient<T>();
            return await client.Save(_URL, obj)
                .ContinueWith((x) =>
                {
                    if (x.Status == TaskStatus.RanToCompletion)
                        return x.Result;
                    else
                        return 0;
                }
            );
        }

        public async Task<T> GetObject<T>(string hostAddr, string database, string procedure, Object obj, bool showLoading = false, bool ignoreNull = true)
        {
            T retVal = default(T);
            string _URL = $"http://{hostAddr}/{database}.dbo.{procedure}/json";

            if (!string.IsNullOrEmpty(procedure))// && obj != null)
            {
                retVal = (T)await new DataApiClient<T>().GetBy(_URL, obj, ignoreNull)
                    .ContinueWith((x) =>
                    {
                        if (x.Status == TaskStatus.RanToCompletion)
                        {
                            StoredProcedureResponse<T> rst = x.Result;

                            if (rst.ResultSets != null && rst.ResultSets.Count > 0 && rst.ResultSets[0].Count > 0)
                                return rst.ResultSets[0].FirstOrDefault();
                            else if (rst.ReturnValue != null)
                            {
                                if (typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(float) || typeof(T) == typeof(double))
                                    return rst.ReturnValue;
                                else
                                    return default(T);
                            }
                            else
                                return default(T);
                        }
                        else return default(T);
                    }
                );
            }

            return retVal;
        }

        public async Task<IList<T>> GetObjectList<T>(string hostAddr, string database, string procedure, Object obj = null, bool showLoading = false)
        {
            IList<T> retVal = default(IList<T>);
            //if (string.IsNullOrEmpty(hostAddr)) hostAddr = App.HostAddr;

            string _URL = $"http://{hostAddr}/{database}.dbo.{procedure}/json";
            if (!string.IsNullOrEmpty(procedure)) //&& obj != null)
            {
                retVal = await new DataApiClient<T>().GetBy(_URL, obj)
                    .ContinueWith((x) =>
                    {
                        if (x.Status == TaskStatus.RanToCompletion)
                            return x.Result?.ResultSets?.Count > 0 ? x.Result.ResultSets[0] : null;
                        else
                            return default(IList<T>);
                    }
                );
            }

            return retVal;
        }

    }
}

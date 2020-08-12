using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisTest.Infrstructure.Cache
{
    /// <summary>
    /// <see cref="Microsoft.ApplicationServer.Caching.DataCache"/>資料快取提供者
    /// </summary>
    public class RedisCacheProvider : IRedisCacheProvider
    {
        private ConnectionMultiplexer _redis;

        //是否在序列化的 JSON 結果裡保留型別
        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        //Redis Cache Server (Master 1)
        //private string _master1 = string.Empty;

        //Redis Cache Server (Slave 1)
        //private string _slave1 = string.Empty;
        /// <summary>
        /// 建立執行個體
        /// 必須設定好 Redis Server for master and slave.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public RedisCacheProvider()
        {
            _redis = RedisState.CreateRedisConnection();
        }

        private IDatabase Current
        {
            get
            {
                return RedisState.Db;
            }
        }
        public void TryPut(string key, object data)
        {
            Remove(key);
            Put(key, data);
        }
        /// <summary>
        /// 建立或置換快取資料值
        /// </summary>
        /// <param name="key">鍵值，如果指定的鍵值不存在則新增一筆快取項目。</param>
        /// <param name="data">資料值</param>
        /// <remarks>資料值必須為可序列化的型別。</remarks>
        public void Put(string key, object data)
        {
            try
            {
                Current.StringSet(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
            }
            catch (RedisConnectionException rex)
            {
                int reTry = 0;
                do
                {
                    //延遲 1 秒鐘.
                    Thread.Sleep(500);
                    try
                    {
                        Current.StringSet(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
                        //成功即跳出迴圈.
                        break;
                    }
                    catch
                    {
                        RedisState.CloseMultiplexer();

                        RedisState.CreateMultiplexer();
                        //失敗則在迴圈中繼續 ReTry
                    }
                    reTry++;

                } while (reTry < 5);

                if (reTry == 5)
                {
                    //紀錄 Log
                    Trace.WriteLine(string.Format("{0}, {1}", rex.GetType().Name, rex.Message));
                }
            }
        }
        /// <summary>
        /// 建立或置換快取資料值
        /// </summary>
        /// <param name="key">鍵值，如果指定的鍵值不存在則新增一筆快取項目。</param>
        /// <param name="data">資料值</param>
        /// <param name="liveTime">存活時間</param>
        /// <remarks>資料值必須為可序列化的型別。</remarks>
        public void Put(string key, object data, TimeSpan liveTime)
        {
            try
            {
                Current.StringSet(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings), liveTime);
            }
            catch (RedisConnectionException rex)
            {
                int reTry = 0;
                do
                {
                    //延遲 1 秒鐘.
                    Thread.Sleep(500);
                    try
                    {
                        Current.StringSet(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings), liveTime);
                        //成功即跳出迴圈.
                        break;
                    }
                    catch
                    {
                        RedisState.CloseMultiplexer();

                        RedisState.CreateMultiplexer();
                        //失敗則在迴圈中繼續 ReTry
                    }
                    reTry++;

                } while (reTry < 5);

                if (reTry == 5)
                {
                    //紀錄 Log，請修改為 e 保網紀錄 Log 的方式.
                    Trace.WriteLine(string.Format("{0}, {1}", rex.GetType().Name, rex.Message));
                }
            }
        }
        /*
	    ///<summary>
	    ///從快取中取得資料
	    ///</summary>
	    public T Get<T>(string key)
		    where T: class
	    {
		    try {
			    var jsonString = Current.StringGet(key);
			    if(!jsonString.IsNull)
			    {
				    return JsonConvert.DeserializeObject<T>(jsonString);
			    }
			    else {
				    return default(T);
			    }
		    }
		    catch(Exception ex) {
			    throw ex;
		    }
	    }
	    */
        /// <summary>
        /// 取得快取資料值
        /// </summary>
        /// <param name="key">鍵值</param>
        /// <returns>資料值，<see langword="null"/> 如果取值失敗。</returns>
        public object Get(string key)
        {
            object result = null;
            try
            {
                RedisValue cacheResult = Current.StringGet(key);
                result = JsonConvert.DeserializeObject(cacheResult, _jsonSerializerSettings);
            }
            catch (RedisConnectionException rex)
            {
                int reTry = 0;
                do
                {
                    //延遲 1 秒鐘.
                    Thread.Sleep(500);
                    try
                    {
                        RedisValue cacheResult = Current.StringGet(key);
                        result = JsonConvert.DeserializeObject(cacheResult, _jsonSerializerSettings);
                        //成功即跳出迴圈.
                        break;
                    }
                    catch
                    {
                        RedisState.CloseMultiplexer();

                        RedisState.CreateMultiplexer();
                        //失敗則在迴圈中繼續 ReTry
                    }
                    reTry++;

                } while (reTry < 5);

                if (reTry == 5)
                {
                    //紀錄 Log，請修改為 e 保網紀錄 Log 的方式.
                    Trace.WriteLine(string.Format("{0}, {1}", rex.GetType().Name, rex.Message));
                }
            }
            catch (Exception ex)
            {
                //紀錄 Log，請修改為 e 保網紀錄 Log 的方式.
                Debug.WriteLine(string.Format("Fail to Get(\"{0}\")! Treat as a null result.\nError Detail:\n", key, ex.ToString()));
            }

            return result;
        }
        /// <summary>
        /// 移除指定的一個快取資料
        /// </summary>
        /// <param name="key">鍵值，如果鍵值不存在將被忽略。</param>
        public void Remove(string key)
        {
            try
            {
                Current.KeyDelete(key);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Fail to Get(\"{0}\")! Treat as a null result.\nError Detail:\n", key, ex.ToString()));
            }
        }

        private long getObjectSize(object o)
        {
            long size = 0;
            using (Stream s = new MemoryStream())
            {
                NetDataContractSerializer ser = new NetDataContractSerializer();
                ser.Serialize(s, o);
                size = s.Length;
            }
            return size;
        }
    }
}

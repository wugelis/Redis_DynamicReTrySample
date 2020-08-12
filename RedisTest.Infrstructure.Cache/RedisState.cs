using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisTest.Infrstructure.Cache
{
    /// <summary>
    /// 建立 Redis 連線物件
    /// </summary>
    public class RedisState
    {
        protected static Lazy<ConnectionMultiplexer> _redisConnection;

        public static IDatabase Db => CreateRedisConnection().GetDatabase();
        //Redis Server
        public static IServer RedisServer;

        private static string _master1 = string.Empty;
        #region Master1 唯讀變數
        public static string Master1
        {
            get
            {
                return _master1;
            }
        }
        #endregion
        private static string _slave1 = string.Empty;
        #region Slave1 唯讀變數
        public static string Slave1
        {
            get
            {
                return _slave1;
            }
        }
        #endregion
        private static int _port;
        #region Port 唯讀變數
        public static int Port
        {
            get
            {
                return _port;
            }
        }
        #endregion
        private static int _portSlave;

        /// <summary>
        /// 取得 Redis Configuration Options 物件.
        /// </summary>
        /// <param name="ssl"></param>
        /// <param name="clientName"></param>
        /// <returns></returns>
        protected static ConfigurationOptions GetConfiguration(string redisConnString, bool ssl)
        {
            var configuration = ConfigurationOptions.Parse(redisConnString);
            configuration.Ssl = ssl;
            //configuration.ClientName = clientName;
            configuration.AbortOnConnectFail = false;
            return configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        static RedisState()
        {
            string redisConnString = GetRedisConnection();

            ConfigurationOptions options = GetConfiguration(redisConnString, false); //ConfigurationOptions.Parse(redisConnStr);

            Connect2Redis(options);

            RedisServer = CreateRedisConnection().GetServer(options.EndPoints.First());
        }

        public static void Connect2Redis(ConfigurationOptions options)
        {
            _redisConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                ConnectionMultiplexer result = null;
                try
                {
                    result = ConnectionMultiplexer.Connect(options);
                }
                catch (RedisConnectionException)
                {
                    result = ConnectionMultiplexer.Connect(options);
                }

                return result;
            });
        }
        /// <summary>
        /// 取得 Redis ConnectionString.
        /// </summary>
        /// <returns></returns>
        public static string GetRedisConnection()
        {
            _master1 = ConfigurationManager.AppSettings["RedisMaster"];

            _slave1 = ConfigurationManager.AppSettings["RedisSlave"];

            _port = ConfigurationManager.AppSettings["Port"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["Port"]) : 6379;

            _portSlave = ConfigurationManager.AppSettings["PortSlave"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["PortSlave"]) : 6380;

            string redisConnString = $"{_master1}:{_port},{_slave1}:{_portSlave},password=gelis123,allowAdmin=true";
            return redisConnString;
        }
        /// <summary>
        /// 取得可操作 Redis 的 Multiplexer 物件.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer CreateRedisConnection()
        {
            return _redisConnection.Value;
        }

        /// <summary>
        /// 建立 Redis Mulitiplexer 物件.
        /// </summary>
        /// <returns></returns>
        public static void CreateMultiplexer()
        {
            ConfigurationOptions options = GetConfiguration(GetRedisConnection(), false);

            Lazy<ConnectionMultiplexer> redisConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                ConnectionMultiplexer result = null;
                try
                {
                    result = ConnectionMultiplexer.Connect(options);
                }
                catch (RedisConnectionException)
                {
                    result = ConnectionMultiplexer.Connect(options);
                }

                return result;
            });

            _redisConnection = redisConnection;
            //lastReconnectTime = DateTimeOffset.UtcNow;
        }
        /// <summary>
        /// 關閉 Redis Mulitiplexer 物件.
        /// </summary>
        /// <param name="oldMultiplexer"></param>
        public static void CloseMultiplexer(Lazy<ConnectionMultiplexer> oldMultiplexer)
        {
            if (oldMultiplexer != null)
            {
                try
                {
                    oldMultiplexer.Value.Close();
                }
                catch (Exception ex)
                {
                    //如果基礎網路連接已經關閉，再進行關閉可能出現錯誤，如果是因為 Close() 引發的錯誤這裡不進行處理
                }
            }
        }
        /// <summary>
        /// 關閉 Redis Mulitiplexer 物件.
        /// </summary>
        public static void CloseMultiplexer()
        {
            if (_redisConnection.IsValueCreated)
            {
                CloseMultiplexer(_redisConnection);
            }

        }
    }
}

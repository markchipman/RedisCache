using StackExchange.Redis;
using System.Net;

namespace RedisCache
{
    public class RedisContext : IRedisContext
    {
        private volatile ConnectionMultiplexer connection;
        private readonly object _lock = new object();
        private string _connectionString;

        /// <summary>
        /// Set connection
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        
        /// <summary>
        /// Get redis connection
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnection()
        {
            if (connection != null && connection.IsConnected) return connection;

            lock (_lock)
            {
                if (connection != null && connection.IsConnected) return connection;

                if (connection != null)
                    connection.Dispose();

                /*var configuration = new ConfigurationOptions()
                {
                    AbortOnConnectFail = false,
                    EndPoints =
                    {
                        {"185.136.205.78", 18227 }
                    }
                };*/

                connection = ConnectionMultiplexer.Connect(_connectionString);
            }

            return connection;
        }

        /// <summary>
        /// Get redis database
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public IDatabase GetDatabase(int? database = default(int?))
        {
            return GetConnection().GetDatabase(database ?? -1);
        }

        /// <summary>
        /// Get redis server by EndPoint
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public IServer Server(EndPoint endPoint)
        {
            return GetConnection().GetServer(endPoint);
        }

        /// <summary>
        /// Get redis server all EndPoint
        /// </summary>
        /// <returns></returns>
        public EndPoint[] GetEndpoints()
        {
            return GetConnection().GetEndPoints();
        }

        /// <summary>
        /// Flush all database or one database
        /// </summary>
        /// <param name="database"></param>
        public void FlushDb(int? database = default(int?))
        {
            var endPoints = GetEndpoints();
            foreach (var endpoint in endPoints)
                Server(endpoint).FlushDatabase(database ?? -1);
        }

        /// <summary>
        /// Dispose redis connection
        /// </summary>
        public void Dispose()
        {
            if (connection != null)
                connection.Dispose();
        }
    }
}
using StackExchange.Redis;
using System;
using System.Net;

namespace RedisCache
{
    public interface IRedisContext: IDisposable
    {
        string ConnectionString { get; set; }
        IDatabase GetDatabase(int? database = null);
        IServer Server(EndPoint endPoint);
        EndPoint[] GetEndpoints();
        void FlushDb(int? database = null);
    }
}
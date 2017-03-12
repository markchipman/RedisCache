using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Text;

namespace RedisCache
{
    public class RedisCacheManager : ICacheManager
    {
        #region Fields
        private readonly IDatabase database;
        private readonly IRedisContext context;
        #endregion

        #region Ctor
        public RedisCacheManager()
        {
            context = new RedisContext();
            context.ConnectionString = ConfigurationManager.ConnectionStrings["RedisServer"].ConnectionString;
            database = context.GetDatabase();
        }
        #endregion

        /// <summary>
        /// Serialize object to byte array
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        /// <summary>
        /// Deserialize byte array data to Generic(T) type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        protected virtual T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject == null)
                return default(T);

            var jsonString = Encoding.UTF8.GetString(serializedObject);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Get cache data by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var value = database.StringGetAsync(key).Result;
            if (!value.HasValue)
                return default(T);

            return Deserialize<T>(value);
        }

        /// <summary>
        /// Set object data with cache time(minutes) by key
        /// Default cache time 1 hour(60 minutes)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cacheMinutes"></param>
        public void Set(string key, object data, int cacheMinutes = 60)
        {
            if (data == null)
                return;

            var entry = Serialize(data);
            var expire = TimeSpan.FromMinutes(cacheMinutes);
            database.StringSetAsync(key, entry, expire);
        }

        /// <summary>
        /// If is set return true by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsSet(string key)
        {
            return database.KeyExistsAsync(key).Result;
        }

        /// <summary>
        /// Remove item by cache key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            database.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Remove item by pattern
        /// </summary>
        /// <param name="pattern"></param>
        public void RemoveByPattern(string pattern)
        {
            foreach (var item in context.GetEndpoints())
            {
                var server = context.Server(item);
                var keys = server.Keys(database: database.Database, pattern: "*" + pattern + "*");
                foreach (var key in keys)
                    Remove(key);
            }
        }

        /// <summary>
        /// Clear all cache data
        /// </summary>
        public void Clear()
        {
            foreach (var item in context.GetEndpoints())
            {
                var server = context.Server(item);
                var keys = server.Keys(database: database.Database);
                foreach (var key in keys)
                    Remove(key);
            }
        }
    }
}
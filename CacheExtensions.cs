using System;

namespace RedisCache
{
    public static class CacheExtensions
    {
        /// <summary>
        /// If cache data not exist set to cache and return data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire)
        {
            return Get(cacheManager, key, acquire, 300);
        }
        
        /// <summary>
        /// If cache data not exist set to cache with cacheTime and return data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <param name="cacheTime"></param>
        /// <returns></returns>
        public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire, int cacheTime = 300)
        {
            if (cacheManager.IsSet(key))
                return cacheManager.Get<T>(key);

            var result = acquire();
            cacheManager.Set(key, result, cacheTime);

            return result;
        }
    }
}

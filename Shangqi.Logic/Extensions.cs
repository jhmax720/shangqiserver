using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Shangqi.Logic
{
    public static class Extensions
    {
        public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
            where T : class, new()
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, value);
                await cache.SetAsync(key, stream.ToArray(), options);
            }
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
            where T : class, new()
        {
            var data = await cache.GetAsync(key);
            if (data != null)
            {
                using (var stream = new MemoryStream(data))
                {
                    {
                        return (T)new BinaryFormatter().Deserialize(stream);
                    }
                }
            }
            else
            {
                return null;
            }
            
        }


        public static async Task SetAsyncString(this IDistributedCache cache, string key, string value, DistributedCacheEntryOptions options)
        {
            await cache.SetAsync(key, Encoding.UTF8.GetBytes(value), options);
        }

        public static async Task<string> GetStringAsync(this IDistributedCache cache, string key)
        {

            var data = await cache.GetAsync(key);
            if (data != null)
            {
                return Encoding.UTF8.GetString(data);
            }
            return null;
        }
    }
}

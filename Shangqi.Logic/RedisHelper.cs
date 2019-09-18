using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Shangqi.Logic.Model;

namespace Shangqi.Logic
{
    public class RedisHelper
    {
        private IDistributedCache _cache;

        private IDictionary<string, IList<RegisteredCarModel>> _carCollection; 

        private RedisHelper()
        {
            _cache = new RedisCache(new MyRedisOptions());
        }
        private static RedisHelper _instance = null;
        public static RedisHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RedisHelper();
                }
                return _instance;
            }
        }

        public async Task SetCache<T>(string key, T model) where T : class, new()
        {
            //
            await _cache.SetAsync<T>(key, model, new DistributedCacheEntryOptions());
        }

        public async Task<bool>TryGetCarList(string key, RegisteredCarModel model)
        {
            return true;
        }

        public void SetNormalCache(string key, byte[] value)
        {
            _cache.Set(key,value, new DistributedCacheEntryOptions());
        }
        public byte[] GetNormalItem(string key)
        {
            return _cache.Get(key);
        }

        public async Task<T> GetCacheItem<T>(string key) where T : class, new()
        {
           return await _cache.GetAsync<T>(key);
        }
        public void ClearKey(string key)
        {
            _cache.Remove(key);
            
        }
        //cache.SetAsync<HeartBeatModel>(str, model, new DistributedCacheEntryOptions()).Wait();

        //HeartBeatModel p = cache.GetAsync<HeartBeatModel>(str).Result;
    }

    public class MyRedisOptions : IOptions<RedisCacheOptions>
    {

        public RedisCacheOptions Value => new RedisCacheOptions
        {
            Configuration = "127.0.0.1:6379",
            InstanceName = "shangqi"
        };
    }
}

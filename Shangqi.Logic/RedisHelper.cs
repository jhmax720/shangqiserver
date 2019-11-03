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

        public async Task<List<CachedRecordingModel>> GetCurrentCarsInCache()
        {

            var l =await _cache.GetAsync<List<CachedRecordingModel>>("AllRobots");
            return l;
        }

        public async Task UpdateCurrentCarsInCache(List<CachedRecordingModel> list)
        {
            await _cache.SetAsync<List<CachedRecordingModel>>("AllRobots", list, new DistributedCacheEntryOptions());
        }

        public async Task SetCache<T>(string key, T model) where T : class, new()
        {
            //
            await _cache.SetAsync<T>(key, model, new DistributedCacheEntryOptions());
        }

        //true => added
        //false => not added
        public async Task<CachedRecordingModel> TryGetFromCarList(string key)
        {
            return await _cache.GetAsync<CachedRecordingModel>(key);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Shangqi.Logic.Configuration;
using Shangqi.Logic.Model;
using StackExchange.Redis;

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

        public async Task<IList<CachedRecordingModel>> GetCurrentCarsInCache()
        {

            IList<CachedRecordingModel> _list = new List<CachedRecordingModel>();
            var iL = GetCachedNameIndex();

            foreach (var i in iL)
            {
                var model = await _cache.GetAsync<CachedRecordingModel>($"car_{i}");
                if (model != null)
                {
                    _list.Add(model);
                }
            }
            

            return _list;
        }


        public int[] GetCachedNameIndex()
        {
            var str = _cache.GetString(Const.ROBOT_INDEX);
            if (str != null)
            {
                var split = str.Split(";").Where(x=>!string.IsNullOrEmpty(x));
                return split.Select(int.Parse).Distinct().ToArray();
            }

            return new int[0];

        }

        public void AddCachedNameIndex(int carName)
        {
            var str = _cache.GetString(Const.ROBOT_INDEX);
            if (str == null) str = string.Empty;
            str +=  carName + ";";

            _cache.SetString(Const.ROBOT_INDEX, str);

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Shangqi.Logic.Model;


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


        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //:::                                                                         :::
        //:::  This routine calculates the distance between two points (given the     :::
        //:::  latitude/longitude of those points). It is being used to calculate     :::
        //:::  the distance between two locations using GeoDataSource(TM) products    :::
        //:::                                                                         :::
        //:::  Definitions:                                                           :::
        //:::    South latitudes are negative, east longitudes are positive           :::
        //:::                                                                         :::
        //:::  Passed to function:                                                    :::
        //:::    lat1, lon1 = Latitude and Longitude of point 1 (in decimal degrees)  :::
        //:::    lat2, lon2 = Latitude and Longitude of point 2 (in decimal degrees)  :::
        //:::    unit = the unit you desire for results                               :::
        //:::           where: 'M' is statute miles (default)                         :::
        //:::                  'K' is kilometers                                      :::
        //:::                  'N' is nautical miles                                  :::
        //:::                                                                         :::
        //:::  Worldwide cities and other features databases with latitude longitude  :::
        //:::  are available at https://www.geodatasource.com                         :::
        //:::                                                                         :::
        //:::  For enquiries, please contact sales@geodatasource.com                  :::
        //:::                                                                         :::
        //:::  Official Web site: https://www.geodatasource.com                       :::
        //:::                                                                         :::
        //:::           GeoDataSource.com (C) All Rights Reserved 2018                :::
        //:::                                                                         :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public static bool IsInRange(this CoordinateModel model, string longitude, string latitude)
        {
            var long1 = Convert.ToDouble(model.Longitude);
            var lat1 = Convert.ToDouble(model.Latitude);

            var long2 = Convert.ToDouble(longitude);
            var lat2 = Convert.ToDouble(latitude);

            var dist = GeoHelper.Distance(lat1, long1, lat2, long2, 'K');

            //todo make this into config
            //3 meters range
            return dist < 0.003;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using StackExchange.Redis;

namespace VSMarketplaceBadge
{
    public class RedisClient
    {
        private static readonly IDatabase Database;

        static RedisClient()
        {
            var redisUrl = ConfigurationManager.AppSettings.Get("REDISTOGO_URL");

            if (redisUrl != null)
            {
                var uri = new Uri(redisUrl);
                var userInfo = uri.UserInfo.Split(':');
                var conf = new ConfigurationOptions
                {
                    EndPoints = { { uri.Host, uri.Port } },
                    ClientName = userInfo[0],
                    Password = userInfo[1]
                };
                Loggly.SendDebug(new { Url = redisUrl, Conf = conf }).FireAndForget();
                try
                {
                    var redis = ConnectionMultiplexer.Connect(conf);
                    Database = redis.GetDatabase();
                }
                catch (Exception e)
                {
                    Loggly.SendError(e).FireAndForget();
                }
            }
        }

        public static async Task<byte[]> GetImage(string key)
        {
            if (Database == null) return null;
            try
            {
                return await Database?.StringGetAsync(key);
            }
            catch (Exception e)
            {
                Loggly.SendError(e).FireAndForget();
                return null;
            }
        }

        public static void SetImage(string key, byte[] image)
        {
            try
            {
                Database?.StringSet(key, image, TimeSpan.FromDays(1), flags: CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                Loggly.SendError(e).FireAndForget();
            }
        }
    }
}
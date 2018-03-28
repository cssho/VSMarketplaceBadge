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

                var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
                {
                    EndPoints = { { uri.Host, uri.Port } },
                    ClientName = userInfo[0],
                    Password = userInfo[1]
                });
                Database = redis.GetDatabase();
            }
        }

        public static async Task<byte[]> GetImage(string key)
        {
            if (Database == null) return null;
            return await Database?.StringGetAsync(key);
        }

        public static void SetImage(string key, byte[] image)
            => Database?.StringSet(key, image, TimeSpan.FromDays(1), flags: CommandFlags.FireAndForget);

    }
}
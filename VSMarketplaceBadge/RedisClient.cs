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
        private static Lazy<ConfigurationOptions> configOptions
            = new Lazy<ConfigurationOptions>(() =>
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
                        Password = userInfo[1],
                        AbortOnConnectFail = false
                    };
                    Loggly.SendDebug(new
                    {
                        Url = redisUrl,
                        Conf = conf
                    }).FireAndForget();
                    return conf;
                }

                return null;
            });

        private static Lazy<ConnectionMultiplexer> conn
            = new Lazy<ConnectionMultiplexer>(
                () =>
                {
                    try
                    {
                        return configOptions.Value == null ? null : ConnectionMultiplexer.Connect(configOptions.Value);
                    }
                    catch (Exception e)
                    {
                        Loggly.SendError(e).FireAndForget();
                    }

                    return null;
                });
        private static ConnectionMultiplexer SafeConn => conn.Value;

        public static async Task<byte[]> GetImage(string key)
        {
            
            try
            {
                var db = SafeConn?.GetDatabase();
                if (db == null) return null;
                return await db.StringGetAsync(key);
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
                SafeConn?.GetDatabase().StringSet(key, image, TimeSpan.FromDays(1), flags: CommandFlags.FireAndForget);
            }
            catch (Exception e)
            {
                Loggly.SendError(e).FireAndForget();
            }
        }
    }
}
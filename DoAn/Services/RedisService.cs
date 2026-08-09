using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn.Services
{
    public class RedisService
    {
        private static ConnectionMultiplexer redis =
           ConnectionMultiplexer.Connect("localhost:6379");

        public static IDatabase GetDatabase()
        {
            return redis.GetDatabase();
        }
    }
}
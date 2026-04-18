using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    

public class RedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConfiguration config)
        {
            var redis = ConnectionMultiplexer.Connect(
                config["Redis:ConnectionString"]
            );

            _db = redis.GetDatabase();
        }

        public async Task SetAsync(string key, string value, TimeSpan expiry)
        {
            await _db.StringSetAsync(key, value, expiry);
        }

        public async Task<string?> GetAsync(string key)
        {
            return await _db.StringGetAsync(key);
        }

        public async Task RemoveAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }
    }

}


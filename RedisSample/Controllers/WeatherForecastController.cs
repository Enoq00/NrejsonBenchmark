using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NReJSON;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RedisSample.Controllers
{
    public sealed class TestJsonSerializer : ISerializerProxy
    {
        public TResult Deserialize<TResult>(RedisResult serializedValue)
        {
            return JsonConvert.DeserializeObject<TResult>(serializedValue.ToString());
        }

        public string Serialize<TObjectType>(TObjectType obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private HashEntry[] Jsonconvert;
        private ConnectionMultiplexer redis;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            redis = ConnectionMultiplexer.Connect("localhost:6379,allowadmin=true");
            NReJSONSerializer.SerializerProxy = new TestJsonSerializer();

            _logger = logger;
        }

        [HttpGet]
        public async Task<string> SetAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            var db = redis.GetDatabase();

            for (int i = 0; i < 1000; i++)
            {
                var wf = new Table
                {
                    Date = DateTime.UtcNow,
                    Summary = "Summary of wf",
                    TemperatureC = 35,
                    Partner = new Partner() { Id = 18484, Status = new Status() { Id = 5151 } }
                };

                var t = await db.JsonSetAsync($"wf_{i}", wf);
            }

            sw.Stop();

            return sw.ElapsedMilliseconds.ToString();
        }

        [HttpGet("set")]
        public async Task UpdateAsync()
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < 1000; i++)
            {
                var t = await db.JsonSetAsync($"wf_{i}", DateTime.UtcNow, "Date");
            }
        }

        [HttpGet("get/{i}")]
        public async Task<JsonResult> GetAsync(int i)
        {
            var db = redis.GetDatabase();

            var t = await db.JsonGetAsync<Table>($"wf_{i}");

            return new JsonResult(t);
        }
    }
}

using Newtonsoft.Json;
using NReJSON;
using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NreJsonBenchmark
{
    static class Program
    {
        private const int IterationCount = 10000;

        static async Task Main(string[] args)
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379,allowadmin=true");
            NReJSONSerializer.SerializerProxy = new TestJsonSerializer();

            redis.GetServer("localhost:6379").FlushDatabase();

            await Run(SetObjects, redis);

            await Run(SetJsonObjects, redis);

            await Run(UpdateObjects, redis);

            await Run(UpdateJsonObjects, redis);

            await Run(GetObjects, redis);

            await Run(GetJsonObjects, redis);
        }


        static async Task SetObjects(ConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < IterationCount; i++)
            {
                var wf = new Table
                {
                    Date = DateTime.UtcNow,
                    Summary = "Summary of wf",
                    TemperatureC = 35,
                    Partner = new Partner() { Id = 18484, Status = new Status() { Id = 5151 } }
                };

                var t = await db.HashSetAsync($"wf_{i}", i, JsonConvert.SerializeObject(wf));
            }
        }

        static async Task UpdateObjects(ConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < IterationCount; i++)
            {
                var t = db.HashGet($"wf_{i}", i);

                var obj = JsonConvert.DeserializeObject<Table>(t);
                obj.Summary = "af";

                await db.HashSetAsync($"wf_{i}", i, JsonConvert.SerializeObject(obj));
            }
        }

        static async Task GetObjects(ConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < IterationCount; i++)
            {
                var t = await db.HashGetAsync($"wf_{i}", i);

                var obj = JsonConvert.DeserializeObject<Table>(t);
            }
        }

        static async Task SetJsonObjects(ConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < IterationCount; i++)
            {
                var wf = new Table
                {
                    Date = DateTime.UtcNow,
                    Summary = "Summary of wf",
                    TemperatureC = 35,
                    Partner = new Partner() { Id = 18484, Status = new Status() { Id = 5151 } }
                };

                var t = await db.JsonSetAsync($"wfjson_{i}", wf);
            }
        }

        static async Task UpdateJsonObjects(ConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < IterationCount; i++)
            {
                await db.JsonSetAsync($"wfjson_{i}", DateTime.UtcNow, "Date");
            }
        }

        static async Task GetJsonObjects(ConnectionMultiplexer redis)
        {
            var db = redis.GetDatabase();

            for (int i = 0; i < IterationCount; i++)
            {
                var t = await db.JsonGetAsync<int>($"wfjson_{i}", "Partner.Status.Id");
            }
        }

        static async Task Run(Func<ConnectionMultiplexer, Task> t, ConnectionMultiplexer connectionMultiplexer)
        {
            var sw = Stopwatch.StartNew();

            await t(connectionMultiplexer);

            sw.Stop();

            Console.WriteLine("{0} Time taken: {1}ms", t.Method.Name, sw.Elapsed.TotalMilliseconds);
        }
    }


    public sealed class TestJsonSerializer : ISerializerProxy
    {
        public TResult Deserialize<TResult>(RedisResult serializedValue)
        {
            return JsonConvert.DeserializeObject<TResult>(serializedValue.ToString());
        }

        public string Serialize<TObjectType>(TObjectType obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }

    public class Table
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public string Summary { get; set; }

        public Partner Partner { get; set; }
    }

    public class Partner
    {
        public int Id { get; set; }

        public Status Status { get; set; }
    }

    public class Status
    {
        public int Id { get; set; }
    }
}

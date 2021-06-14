using System;

namespace RedisSample
{
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

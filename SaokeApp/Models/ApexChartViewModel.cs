using Newtonsoft.Json;

namespace SaokeApp.Models
{
    public class ApexChartViewModel<T,V>
    {
        public List<T> Labels { get; set; }

        public List<ApexChartDataSet<V>> Series { get; set; }
    }

    public class ApexChartDataSet<V>
    {
        public string Name { get; set; }

        public List<V> Data { get; set; }
    }

    public class ApexChartTreeMapDataSet
    {
        [JsonProperty("data")]
        public List<ApexChartTreeMapDataPoint> Data { get; set; }
    }

    public class ApexChartTreeMapDataPoint
    {
        [JsonProperty("x")]
        public string X { get; set; }
        [JsonProperty("y")]
        public int Value { get; set; }

    }
}

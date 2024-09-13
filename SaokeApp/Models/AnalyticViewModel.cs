namespace SaokeApp.Models
{
    public class AnalyticViewModel
    {
        public long TotalDonateAmount { get; set; }

        public int TotalPersonCount { get; set; }

        public long MaxAmount { get; set; }

        public ApexChartTreeMapDataSet DistributedAmount { get; set; }

        public ApexChartViewModel<DateTime, long> DonateTimeSeries { get; set; }
    }
}

using NodaTime;
using SaokeApp.Entities;

namespace SaokeApp.Models
{
    public class IndexViewModel
    {
        public string Search { get; set; }

        public List<DonateTrackViewModel> DonateTracks { get; set; } = new List<DonateTrackViewModel>();

        public int TotalCount { get; set; } = 0;

        public int ItemsPerPage { get; set; } = 20;

        public int PageNumber { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)ItemsPerPage);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class DonateTrackViewModel
    {
        public DateTime CreatedAt { get; set; }

        public long Amount { get; set; }

        public string Message { get; set; } = string.Empty;

        public string TransactionId { get; set; } = string.Empty;
    }
}

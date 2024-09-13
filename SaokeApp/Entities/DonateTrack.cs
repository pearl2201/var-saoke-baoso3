using NodaTime;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;

namespace SaokeApp.Entities
{
    public class DonateTrack
    {
        [Key]
        public int Id { get; set; }
        public Instant CreatedAt { get; set; }

        public long Amount { get; set; }

        public string Message { get; set; } = string.Empty;

        public string TransactionId { get; set; } = string.Empty;

        public NpgsqlTsVector SearchVector { get; set; }
    }
}

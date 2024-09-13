using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using System.Net.WebSockets;

namespace SaokeApp.Data
{
    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                if (_context.Database.IsNpgsql())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task TrySeedAsync()
        {
            var donateTrackCount = await _context.DonateTracks.CountAsync();
            if (donateTrackCount == 0)
            {
                var text = File.ReadAllText("mttq.json");
                var importDonateRecords = JsonConvert.DeserializeObject<List<NationalSupportRecord>>(text);
                foreach (var donateRecord in importDonateRecords)
                {
                    var time = donateRecord.CreatedAt;
                    _context.DonateTracks.Add(new Entities.DonateTrack
                    {
                        Amount = donateRecord.Amount,
                        CreatedAt = ToUtcInstance(time),
                        Message = donateRecord.Message,
                        TransactionId = donateRecord.TransactionId
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        private class NationalSupportRecord
        {
            public DateTime CreatedAt { get; set; }

            public long Amount { get; set; }

            public string Message { get; set; }

            public string TransactionId { get; set; }
        }

        public static Instant ToUtcInstance(DateTime time) => Instant.FromDateTimeUtc(DateTime.SpecifyKind(time, DateTimeKind.Utc));
    }
}

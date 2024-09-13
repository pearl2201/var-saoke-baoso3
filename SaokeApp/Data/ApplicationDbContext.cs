using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaokeApp.Entities;

namespace SaokeApp.Data
{
    public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<DonateTrack> DonateTracks { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
            )
    : base(options)
        {
         
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DonateTrack>()
                .HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    "vietnamese",  // Text search config
                    p => new { p.TransactionId, p.Amount, p.Message })  // Included properties
                .HasIndex(p => p.SearchVector)
                .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
        }

    }
}

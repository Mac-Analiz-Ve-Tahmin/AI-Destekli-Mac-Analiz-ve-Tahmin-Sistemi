using Microsoft.EntityFrameworkCore;
using MatchAnalysisSystem.Core.Entities;
namespace MatchAnalysisSystem.DataAccess
{
    public class MatchDbContext : DbContext
    {
        public MatchDbContext(DbContextOptions<MatchDbContext> options) : base(options)
        {
        }

        // Veri tabanında oluşacak tablolarımız
        public DbSet<Team> Teams { get; set; }
        public DbSet<MatchHistory> MatchHistories { get; set; }

        // MatchDbContext.cs içine eklenecek satır:
        public DbSet<MatchPrediction> MatchPredictions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Maç geçmişi tablosunda Ev Sahibi ve Deplasman takımlarının 
            // çakışmaması için SQL Server ilişkilerini yapılandırıyoruz.
            modelBuilder.Entity<MatchHistory>()
                .HasOne(m => m.HomeTeam)
                .WithMany()
                .HasForeignKey(m => m.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchHistory>()
                .HasOne(m => m.AwayTeam)
                .WithMany()
                .HasForeignKey(m => m.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MatchPrediction>(entity =>
            {
                entity.Property(p => p.CacheKey).HasMaxLength(64).IsRequired();
                entity.HasIndex(p => p.CacheKey).IsUnique();
            });
        }
    }
}
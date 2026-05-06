using Microsoft.EntityFrameworkCore;
using MatchAnalysisSystem.Core;

namespace MatchAnalysisSystem.DataAccess;

public class MatchDbContext : DbContext
{
    // Veritabanındaki "Matches" tablosunu temsil eder
    public DbSet<Match> Matches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Kendi bilgisayarındaki SQL Server bağlantı adresini buraya yazacağız
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MatchAnalysisDb;Trusted_Connection=True;");
    }
}
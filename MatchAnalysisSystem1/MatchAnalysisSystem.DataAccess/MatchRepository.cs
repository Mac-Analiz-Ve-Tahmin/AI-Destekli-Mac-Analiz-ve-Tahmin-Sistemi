using MatchAnalysisSystem.Core;

namespace MatchAnalysisSystem.DataAccess;

public class MatchRepository : IMatchRepository
{
    // Veritabanı köprüsü (DbContext) örneği
    private readonly MatchDbContext _context = new MatchDbContext();

    public List<Match> GetAllMatches()
    {
        // Veritabanındaki tüm maçları liste olarak döner
        return _context.Matches.ToList();
    }

    public void Add(Match match)
    {
        // Yeni maçı tabloya ekler ve fiziksel olarak kaydeder
        _context.Matches.Add(match);
        _context.SaveChanges();
    }
}
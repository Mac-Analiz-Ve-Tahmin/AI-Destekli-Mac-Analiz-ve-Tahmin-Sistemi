using MatchAnalysisSystem.Core;

namespace MatchAnalysisSystem.DataAccess;

public interface IMatchRepository
{
    List<Match> GetAllMatches(); // Veritabanındaki tüm maçları getirir
    void Add(Match match);       // Veritabanına yeni maç ekler
}
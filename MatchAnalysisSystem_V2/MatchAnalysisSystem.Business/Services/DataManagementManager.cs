using MatchAnalysisSystem.DataAccess;
using MatchAnalysisSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchAnalysisSystem.Business.Services
{
    public class DataManagementManager
    {
        private readonly MatchDbContext _context;

        public DataManagementManager(MatchDbContext context)
        {
            _context = context;
        }

        // 1. Yeni Takım Ekleme
        public async Task<Team> AddTeamAsync(string teamName, double attack = 1.0, double defense = 1.0)
        {
            var team = new Team { Name = teamName, AttackRating = attack, DefenseRating = defense };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team;
        }

        // 2. Tüm Takımları Listeleme
        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams.ToListAsync();
        }

        // 3. Maç Geçmişi Kaydetme
        public async Task<MatchHistory> AddMatchHistoryAsync(MatchHistory match)
        {
            _context.MatchHistories.Add(match);
            await _context.SaveChangesAsync();
            return match;
        }

        // 4. Bir Takımın Son Maçlarını Getirme (EMA hesaplamasında kullanacağız)
        public async Task<List<MatchHistory>> GetTeamLastMatchesAsync(int teamId, int count = 5)
        {
            return await _context.MatchHistories
                .Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .OrderByDescending(m => m.MatchDate)
                .Take(count)
                .ToListAsync();
        }
    }
}
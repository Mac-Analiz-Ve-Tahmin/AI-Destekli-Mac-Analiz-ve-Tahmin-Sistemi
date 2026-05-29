using MatchAnalysisSystem.Core.Entities;
using MatchAnalysisSystem.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace MatchAnalysisSystem.Business.Services
{
    public class MatchPredictionService
    {
        private readonly MatchDbContext _context;
        private readonly FootballApiService _apiService;

        public MatchPredictionService(MatchDbContext context, FootballApiService apiService)
        {
            _context = context;
            _apiService = apiService;
        }

        public static string BuildCacheKey(int homeTeamId, int awayTeamId, DateTime? date = null)
        {
            var day = date ?? DateTime.Today;
            return $"{homeTeamId}_{awayTeamId}_{day:yyyyMMdd}";
        }

        public async Task<PredictionResult?> GetCachedPredictionAsync(
            int homeTeamId,
            int awayTeamId,
            CancellationToken cancellationToken = default)
        {
            var todaySuffix = $"_{DateTime.Today:yyyyMMdd}";
            var cacheKey = BuildCacheKey(homeTeamId, awayTeamId);

            var cached = await _context.MatchPredictions
                .AsNoTracking()
                .Where(p =>
                    p.CacheKey == cacheKey ||
                    (p.HomeTeamId == homeTeamId && p.AwayTeamId == awayTeamId && p.CacheKey.EndsWith(todaySuffix)))
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return cached == null ? null : ToPredictionResult(cached);
        }

        public async Task<PredictionResult> GetOrCreatePredictionAsync(
            int homeTeamId,
            int awayTeamId,
            string homeTeamName,
            string awayTeamName,
            CancellationToken cancellationToken = default)
        {
            var existing = await GetCachedPredictionAsync(homeTeamId, awayTeamId, cancellationToken);
            if (existing != null)
                return existing;

            var cacheKey = BuildCacheKey(homeTeamId, awayTeamId);
            var computed = await ComputePredictionAsync(
                homeTeamId, awayTeamId, homeTeamName, awayTeamName, cancellationToken);

            try
            {
                _context.MatchPredictions.Add(ToEntity(
                    cacheKey, homeTeamId, awayTeamId, homeTeamName, awayTeamName, computed));
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                var raceWinner = await GetCachedPredictionAsync(homeTeamId, awayTeamId, cancellationToken);
                if (raceWinner != null)
                    return raceWinner;
                throw;
            }

            return computed;
        }

        public async Task<PredictionResult> ComputePredictionAsync(
            int homeTeamId,
            int awayTeamId,
            string homeTeamName,
            string awayTeamName,
            CancellationToken cancellationToken = default)
        {
            var fixtures = await _apiService.GetDailyFixturesAsync();
            var fixture = fixtures?.FirstOrDefault(f =>
                f.HomeTeamId == homeTeamId && f.AwayTeamId == awayTeamId);

            string homeParticipantId = fixture?.HomeParticipantId ?? string.Empty;
            string awayParticipantId = fixture?.AwayParticipantId ?? string.Empty;

            var homeTeam = new Team
            {
                Id = homeTeamId,
                Name = homeTeamName,
                AttackRating = 1.0,
                DefenseRating = 1.0
            };
            var awayTeam = new Team
            {
                Id = awayTeamId,
                Name = awayTeamName,
                AttackRating = 1.0,
                DefenseRating = 1.0
            };

            var homeMatches = await BuildTeamMatchHistoryAsync(homeTeamId, homeParticipantId);
            var awayMatches = await BuildTeamMatchHistoryAsync(awayTeamId, awayParticipantId);

            var analysisManager = new PoissonAnalysisManager();
            return analysisManager.PredictMatchDinamik(homeTeam, homeMatches, awayTeam, awayMatches);
        }

        private async Task<List<MatchHistory>> BuildTeamMatchHistoryAsync(int teamStableId, string participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId))
                return new List<MatchHistory>();

            var raw = await _apiService.GetTeamLiveMatchHistoryAsync(participantId);
            return raw
                .Where(m => m.HomeTeamId == teamStableId || m.AwayTeamId == teamStableId)
                .Take(5)
                .ToList();
        }

        private static MatchPrediction ToEntity(
            string cacheKey,
            int homeTeamId,
            int awayTeamId,
            string homeTeamName,
            string awayTeamName,
            PredictionResult result)
        {
            return new MatchPrediction
            {
                CacheKey = cacheKey,
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                HomeTeamName = homeTeamName,
                AwayTeamName = awayTeamName,
                ExpectedHomeGoals = result.ExpectedHomeGoals,
                ExpectedAwayGoals = result.ExpectedAwayGoals,
                MostLikelyScore = result.MostLikelyScore,
                HomeWinProbability = (int)Math.Round(result.HomeWinProbability),
                DrawProbability = (int)Math.Round(result.DrawProbability),
                AwayWinProbability = (int)Math.Round(result.AwayWinProbability),
                Over25Probability = (int)Math.Round(result.Over25Probability),
                Under25Probability = (int)Math.Round(result.Under25Probability),
                CreatedAt = DateTime.UtcNow
            };
        }

        private static PredictionResult ToPredictionResult(MatchPrediction entity)
        {
            return new PredictionResult
            {
                MatchName = $"{entity.HomeTeamName} vs {entity.AwayTeamName} (Profesyonel AI Analizi)",
                ExpectedHomeGoals = entity.ExpectedHomeGoals,
                ExpectedAwayGoals = entity.ExpectedAwayGoals,
                MostLikelyScore = entity.MostLikelyScore,
                HomeWinProbability = entity.HomeWinProbability,
                DrawProbability = entity.DrawProbability,
                AwayWinProbability = entity.AwayWinProbability,
                Over25Probability = entity.Over25Probability,
                Under25Probability = entity.Under25Probability
            };
        }
    }
}

using System;

namespace MatchAnalysisSystem.Core.Entities
{
    public class MatchPrediction
    {
        public int Id { get; set; }

        // Bu bizim buluttaki kilit anahtarımız (Örn: "101_102_20240529")
        public string CacheKey { get; set; } = string.Empty;

        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }

        public double ExpectedHomeGoals { get; set; }
        public double ExpectedAwayGoals { get; set; }
        public string MostLikelyScore { get; set; }

        public int HomeWinProbability { get; set; }
        public int DrawProbability { get; set; }
        public int AwayWinProbability { get; set; }
        public int Over25Probability { get; set; }
        public int Under25Probability { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
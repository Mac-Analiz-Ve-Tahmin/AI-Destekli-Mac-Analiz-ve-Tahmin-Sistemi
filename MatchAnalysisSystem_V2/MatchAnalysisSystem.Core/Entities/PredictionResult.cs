namespace MatchAnalysisSystem.Core.Entities
{
    public class PredictionResult
    {
        public string MatchName { get; set; } = string.Empty;

        // Olasılık Sonuçları (Yüzdesel %0 - %100 arası)
        public double HomeWinProbability { get; set; }
        public double DrawProbability { get; set; }
        public double AwayWinProbability { get; set; }

        // Gol Beklentileri (Expected Goals - xG)
        public double ExpectedHomeGoals { get; set; }
        public double ExpectedAwayGoals { get; set; }

        // Alt / Üst Olasılıkları
        public double Over25Probability { get; set; }
        public double Under25Probability { get; set; }

        // En Yüksek Olasılıklı Skor Tahmini
        public string MostLikelyScore { get; set; } = string.Empty;
    }
}
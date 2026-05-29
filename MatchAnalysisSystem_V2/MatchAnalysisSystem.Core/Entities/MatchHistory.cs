namespace MatchAnalysisSystem.Core.Entities
{
    public class MatchHistory
    {
        public int Id { get; set; }
        public DateTime MatchDate { get; set; }

        // Takım İlişkileri
        public int HomeTeamId { get; set; }
        public Team HomeTeam { get; set; } = null!;
        public int AwayTeamId { get; set; }
        public Team AwayTeam { get; set; } = null!;

        // Maç Sonu Skorları
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }

        // Gelişmiş İstatistikler (Yol haritamızdaki korner/kart tahminleri için)
        public int HomeCorners { get; set; }
        public int AwayCorners { get; set; }
        public int HomeShotsOnTarget { get; set; }
        public int AwayShotsOnTarget { get; set; }
    }
}
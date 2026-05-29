namespace MatchAnalysisSystem.Core.Entities
{
    public class LiveFixture
    {
        public int FixtureId { get; set; }
        public DateTime MatchDate { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public int HomeTeamId { get; set; }
        public string HomeTeamName { get; set; } = string.Empty;
        public int AwayTeamId { get; set; }
        public string AwayTeamName { get; set; } = string.Empty;
    }
}
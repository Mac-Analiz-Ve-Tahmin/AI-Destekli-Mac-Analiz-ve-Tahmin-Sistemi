namespace MatchAnalysisSystem.Core.Entities
{
    public class LiveFixture
    {
        public int FixtureId { get; set; }
        public DateTime MatchDate { get; set; }
        public string LeagueName { get; set; } = string.Empty;
        public int HomeTeamId { get; set; }
        public string HomeParticipantId { get; set; } = string.Empty;
        public string HomeTeamName { get; set; } = string.Empty;
        public int AwayTeamId { get; set; }
        public string AwayParticipantId { get; set; } = string.Empty;
        public string AwayTeamName { get; set; } = string.Empty;
    }
}
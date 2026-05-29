namespace MatchAnalysisSystem.Core.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Takımın genel performans reytingleri (Yapay zekanın çarpanları olacak)
        public double AttackRating { get; set; } = 1.0;
        public double DefenseRating { get; set; } = 1.0;
    }
}
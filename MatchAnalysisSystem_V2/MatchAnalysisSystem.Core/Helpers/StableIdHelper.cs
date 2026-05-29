namespace MatchAnalysisSystem.Core.Helpers
{
    /// <summary>
    /// .NET string GetHashCode() oturumlar arası değişir; önbellek ve API ID'leri için sabit hash.
    /// </summary>
    public static class StableIdHelper
    {
        public static int FromParticipantId(string? participantId)
        {
            if (string.IsNullOrWhiteSpace(participantId))
                return 0;

            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in participantId.Trim())
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return (int)(hash % 100_000_000);
            }
        }
    }
}

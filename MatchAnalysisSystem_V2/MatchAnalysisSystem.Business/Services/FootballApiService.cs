using System.Net.Http.Json;
using MatchAnalysisSystem.Core.Entities;
using MatchAnalysisSystem.Core.Helpers;

namespace MatchAnalysisSystem.Business.Services
{
    public class FootballApiService
    {
        private readonly HttpClient _httpClient;

        // RapidAPI Flashlive Sports Kimlik Bilgileri
        private const string ApiKey = "cf8040388dmsh43378df3d07f22fp100f1djsn5718566f5dff";
        private const string ApiHost = "flashlive-sports.p.rapidapi.com";

        // Aylık 500 hakkımızı korumak için 24 saatlik fikstür önbellek (Cache) yapısı
        private static List<LiveFixture>? _cachedFixtures = null;
        private static DateTime _cacheTime = DateTime.MinValue;

        public HttpClient HttpClient => _httpClient;

        public FootballApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://flashlive-sports.p.rapidapi.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", ApiHost);
        }

        // 1. Bugünün Gerçek Zamanlı Maç Bültenini Çeken Metot
        public async Task<List<LiveFixture>> GetDailyFixturesAsync()
        {
            // 🔒 24 Saatlik Sıkı Koruma: Bugün veri çekildiyse internete çıkma, hafızadan getir
            /*if (_cachedFixtures != null && (DateTime.Now - _cacheTime).TotalHours < 24)
            {
                System.Diagnostics.Debug.WriteLine("🛡️ Önbellek Aktif: Fikstür bülteni internet hakkı harcanmadan RAM'den getirildi.");
                return _cachedFixtures;
            }*/

            var fixtures = new List<LiveFixture>();

            try
            {
                // Sunucunun zorunlu tuttuğu tüm parametrelerle bugünün futbol bültenini istiyoruz
                var response = await _httpClient.GetFromJsonAsync<FlashliveResponse>(
                    "events/list?sport_id=1&indent_days=0&timezone=3&locale=en_INT");

                if (response?.DATA != null && response.DATA.Length > 0)
                {
                    // ⚽ MAJÖR LİG VE FUTBOL FİLTRE KALKANI: E-spor, simülasyon ve gereksiz alt ligleri eliyoruz
                    var forbiddenKeywords = new[] { "SRL", "Simulated", "Cyber", "Esoccer", "Electronic", "Virtual", "U19", "U17", "Women", "E-sports", "Amateur", };

                    foreach (var tournament in response.DATA)
                    {
                        if (tournament.EVENTS == null) continue;

                        string fullLeagueName = $"{tournament.COUNTRY_NAME} - {tournament.NAME}";

                        // Lig isminde yukarıdaki yasaklı kelimelerden biri geçiyorsa bu ligi ve maçlarını komple pas geç
                        if (forbiddenKeywords.Any(keyword => fullLeagueName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        foreach (var ev in tournament.EVENTS)
                        {
                            int.TryParse(ev.EVENT_ID?.Replace("g_", ""), out int customId);

                            string homeParticipantId = ev.HOME_PARTICIPANT_IDS != null && ev.HOME_PARTICIPANT_IDS.Length > 0
                                ? ev.HOME_PARTICIPANT_IDS[0] : string.Empty;
                            string awayParticipantId = ev.AWAY_PARTICIPANT_IDS != null && ev.AWAY_PARTICIPANT_IDS.Length > 0
                                ? ev.AWAY_PARTICIPANT_IDS[0] : string.Empty;

                            fixtures.Add(new LiveFixture
                            {
                                FixtureId = customId,
                                MatchDate = ev.START_UTIME > 0
                                    ? DateTimeOffset.FromUnixTimeSeconds(ev.START_UTIME).DateTime.ToLocalTime()
                                    : DateTime.Now,
                                LeagueName = fullLeagueName,
                                HomeParticipantId = homeParticipantId,
                                HomeTeamId = StableIdHelper.FromParticipantId(homeParticipantId),
                                HomeTeamName = ev.HOME_NAME ?? "Ev Sahibi",
                                AwayParticipantId = awayParticipantId,
                                AwayTeamId = StableIdHelper.FromParticipantId(awayParticipantId),
                                AwayTeamName = ev.AWAY_NAME ?? "Deplasman"
                            });
                        }
                    }

                    // Başarılı çekim sonrası veriyi hafızaya alıp zaman damgası vuruyoruz
                    _cachedFixtures = fixtures;
                    _cacheTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Flashlive Fikstür Çekim Hatası: {ex.Message}");
                if (_cachedFixtures != null) return _cachedFixtures;
            }

            return fixtures;
        }

        // 2. Takımın son maçları (Flashlive participant ID ile — int hash değil)
        public async Task<List<MatchHistory>> GetTeamLiveMatchHistoryAsync(string participantId)
        {
            var histories = new List<MatchHistory>();

            if (string.IsNullOrWhiteSpace(participantId))
                return histories;

            try
            {
                var response = await _httpClient.GetFromJsonAsync<FlashliveResponse>(
                    $"teams/results?team_id={Uri.EscapeDataString(participantId)}&sport_id=1&locale=en_INT");

                if (response?.DATA != null && response.DATA.Length > 0)
                {
                    // Takımın oynadığı en güncel turnuvanın maç listesini yakalıyoruz
                    var tournament = response.DATA.FirstOrDefault();
                    if (tournament?.EVENTS != null)
                    {
                        // Analiz motoru (EMA) için son 5 maçı süzüyoruz
                        var last5Events = tournament.EVENTS.Take(5);

                        foreach (var ev in last5Events)
                        {
                            if (!int.TryParse(ev.HOME_SCORE_CURRENT, out int homeGoals) ||
                                !int.TryParse(ev.AWAY_SCORE_CURRENT, out int awayGoals))
                                continue;

                            bool teamWasHome = ev.HOME_PARTICIPANT_IDS != null &&
                                ev.HOME_PARTICIPANT_IDS.Length > 0 &&
                                string.Equals(ev.HOME_PARTICIPANT_IDS[0], participantId, StringComparison.OrdinalIgnoreCase);

                            int teamStableId = StableIdHelper.FromParticipantId(participantId);
                            string opponentParticipant = teamWasHome
                                ? (ev.AWAY_PARTICIPANT_IDS != null && ev.AWAY_PARTICIPANT_IDS.Length > 0
                                    ? ev.AWAY_PARTICIPANT_IDS[0] : string.Empty)
                                : (ev.HOME_PARTICIPANT_IDS != null && ev.HOME_PARTICIPANT_IDS.Length > 0
                                    ? ev.HOME_PARTICIPANT_IDS[0] : string.Empty);
                            int opponentStableId = StableIdHelper.FromParticipantId(opponentParticipant);

                            histories.Add(new MatchHistory
                            {
                                MatchDate = ev.START_UTIME > 0
                                    ? DateTimeOffset.FromUnixTimeSeconds(ev.START_UTIME).DateTime.ToLocalTime()
                                    : DateTime.Now,
                                HomeTeamId = teamWasHome ? teamStableId : opponentStableId,
                                AwayTeamId = teamWasHome ? opponentStableId : teamStableId,
                                HomeGoals = homeGoals,
                                AwayGoals = awayGoals,
                                HomeCorners = Math.Max(0, homeGoals + 2),
                                AwayCorners = Math.Max(0, awayGoals + 1),
                                HomeShotsOnTarget = Math.Max(0, homeGoals + 3),
                                AwayShotsOnTarget = Math.Max(0, awayGoals + 2)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Flashlive Takım Geçmiş Çekim Hatası (ID: {participantId}): {ex.Message}");
            }

            return histories;
        }
    }

    // Flashlive Sports API'den Dönen Ham JSON Şeması Karşılama Sınıfları (Hafıza Haritası)
    public class FlashliveResponse
    {
        public Datum[] DATA { get; set; }
        public META META { get; set; }
        public string LAST_CHANGE_KEY { get; set; }
    }

    public class META
    {
        public BOOKMAKER[] BOOKMAKERS { get; set; }
        public object DATACORE_TRANSLATES { get; set; }
    }

    public class BOOKMAKER
    {
        public int BOOKMAKER_ID { get; set; }
        public int BOOKMAKER_BETTING_TYPE { get; set; }
        public string BOOKMAKER_NAME { get; set; }
    }

    public class Datum
    {
        public string NAME { get; set; }
        public string HEADER { get; set; }
        public string COUNTRY_NAME { get; set; }
        public EVENT[] EVENTS { get; set; }
    }

    public class EVENT
    {
        public string EVENT_ID { get; set; }
        public int START_UTIME { get; set; }
        public string HOME_NAME { get; set; }
        public string AWAY_NAME { get; set; }
        public string HOME_SCORE_CURRENT { get; set; }
        public string AWAY_SCORE_CURRENT { get; set; }
        public string[] HOME_PARTICIPANT_IDS { get; set; }
        public string[] AWAY_PARTICIPANT_IDS { get; set; }
    }
}
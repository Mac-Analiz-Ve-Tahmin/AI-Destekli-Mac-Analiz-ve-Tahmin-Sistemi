// Jira Integration: KAN-21,KAN-22,KAN-23 subtasks completed.
using Microsoft.AspNetCore.Mvc;
using MatchAnalysisSystem.Business.Services;
using MatchAnalysisSystem.Core.Entities;
using MatchAnalysisSystem.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace MatchAnalysisSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly DataManagementManager _dataManager;
        private readonly MatchDbContext _context;
        private readonly FootballApiService _apiService;
        private readonly MatchPredictionService _predictionService;

        public MatchController(
            DataManagementManager dataManager,
            MatchDbContext context,
            FootballApiService apiService,
            MatchPredictionService predictionService)
        {
            _dataManager = dataManager;
            _context = context;
            _apiService = apiService;
            _predictionService = predictionService;
        }

        [HttpPost("add-team")]
        public async Task<IActionResult> AddTeam([FromQuery] string name, [FromQuery] double attack = 1.0, [FromQuery] double defense = 1.0)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("Takım adı boş olamaz.");
            var team = await _dataManager.AddTeamAsync(name, attack, defense);
            return Ok(new { Message = "Takım başarıyla eklendi!", Team = team });
        }

        [HttpGet("teams")]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _dataManager.GetAllTeamsAsync();
            return Ok(teams);
        }

        /// <summary>
        /// Maç tahmini: Azure SQL önbelleğinden okur veya ilk istekte hesaplayıp kaydeder.
        /// Tüm kullanıcılar aynı gün için aynı sonucu görür.
        /// </summary>
        [HttpGet("predict")]
        public async Task<IActionResult> PredictMatchFromDb(
            [FromQuery] int homeTeamId,
            [FromQuery] int awayTeamId,
            [FromQuery] string homeTeamName,
            [FromQuery] string awayTeamName)
        {
            var fixtures = await _apiService.GetDailyFixturesAsync();
            var currentFixture = fixtures?.FirstOrDefault(f => f.HomeTeamId == homeTeamId && f.AwayTeamId == awayTeamId);

            bool isMatchLive = false;
            // isMatchLive = currentFixture != null && currentFixture.Status == "LIVE";

            if (isMatchLive)
            {
                var liveResult = await GenerateLiveMatchAnalysis(homeTeamId, awayTeamId, homeTeamName, awayTeamName);
                return Ok(liveResult);
            }

            var result = await _predictionService.GetOrCreatePredictionAsync(
                homeTeamId, awayTeamId, homeTeamName, awayTeamName);

            return Ok(result);
        }

        private async Task<object> GenerateLiveMatchAnalysis(int homeId, int awayId, string homeName, string awayName)
        {
            int dailySeedModifier = DateTime.Today.DayOfYear;
            int matchSeed = (homeId * 31) + awayId + dailySeedModifier;
            var liveRand = new Random(matchSeed);

            double liveExpectedHome = Math.Round(0.5 + (liveRand.NextDouble() * 3.0), 2);
            double liveExpectedAway = Math.Round(0.3 + (liveRand.NextDouble() * 2.5), 2);

            return new
            {
                ExpectedHomeGoals = liveExpectedHome,
                ExpectedAwayGoals = liveExpectedAway,
                MostLikelyScore = $"{liveRand.Next(0, 4)} - {liveRand.Next(0, 4)}",
                HomeWinProbability = liveRand.Next(20, 60),
                DrawProbability = liveRand.Next(10, 30),
                AwayWinProbability = liveRand.Next(20, 50),
                Over25Probability = liveRand.Next(40, 90),
                Under25Probability = liveRand.Next(10, 60)
            };
        }

        [HttpPost("add-match-history")]
        public async Task<IActionResult> AddMatchHistory([FromBody] MatchHistoryInput input)
        {
            var homeExist = await _context.Teams.AnyAsync(t => t.Id == input.HomeTeamId);
            var awayExist = await _context.Teams.AnyAsync(t => t.Id == input.AwayTeamId);

            if (!homeExist || !awayExist) return BadRequest("Ev sahibi veya deplasman takımı bulunamadı.");

            var match = new MatchHistory
            {
                MatchDate = input.MatchDate,
                HomeTeamId = input.HomeTeamId,
                AwayTeamId = input.AwayTeamId,
                HomeGoals = input.HomeGoals,
                AwayGoals = input.AwayGoals,
                HomeCorners = input.HomeCorners,
                AwayCorners = input.AwayCorners,
                HomeShotsOnTarget = input.HomeShotsOnTarget,
                AwayShotsOnTarget = input.AwayShotsOnTarget
            };

            await _dataManager.AddMatchHistoryAsync(match);
            return Ok(new { Message = "Maç geçmişi başarıyla kaydedildi!", MatchId = match.Id });
        }

        [HttpGet("daily-fixtures")]
        public async Task<IActionResult> GetLiveDailyFixtures()
        {
            var fixtures = await _apiService.GetDailyFixturesAsync();
            return Ok(fixtures);
        }
    }

    public class MatchHistoryInput
    {
        public DateTime MatchDate { get; set; } = DateTime.Now;
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public int HomeCorners { get; set; }
        public int AwayCorners { get; set; }
        public int HomeShotsOnTarget { get; set; }
        public int AwayShotsOnTarget { get; set; }
    }
}

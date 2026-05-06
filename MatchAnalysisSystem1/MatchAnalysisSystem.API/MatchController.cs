using Microsoft.AspNetCore.Mvc;
using MatchAnalysisSystem.Business;
using System.Threading.Tasks;

namespace MatchAnalysisSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly MatchService _matchService;

        public MatchController(MatchService matchService)
        {
            _matchService = matchService;
        }

        // URL'den dinamik olarak Ev Sahibi ve Deplasman takımlarını alıyoruz
        [HttpGet("predict/{homeTeam}/{awayTeam}")]
        public async Task<IActionResult> GetPrediction(string homeTeam, string awayTeam)
        {
            // Kullanıcının girdiği takımları Business katmanına yolluyoruz
            var result = await _matchService.GetMatchPredictionAsync(homeTeam, awayTeam);
            return Ok(result);
        }
    }
}
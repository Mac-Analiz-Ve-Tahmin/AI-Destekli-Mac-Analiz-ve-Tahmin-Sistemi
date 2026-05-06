using Xunit;
using MatchAnalysisSystem.Business;
using System.Net.Http;
using System.Threading.Tasks;

namespace MatchAnalysisSystem.Tests
{
    public class MatchServiceTests
    {
        [Fact] // Bu xUnit'e bunun bir test olduğunu söyler
        public async Task GetMatchPrediction_ReturnsValidResult()
        {
            // Arrange (Hazırlık)
            var httpClient = new HttpClient();
            var service = new MatchService(httpClient);

            // Act (Eylem)
            var result = await service.GetMatchPredictionAsync("Konyaspor", "Fenerbahçe");

            // Assert (Doğrulama - Gelen verinin boş olmadığını kanıtlıyoruz)
            Assert.NotNull(result);
        }
    }
}
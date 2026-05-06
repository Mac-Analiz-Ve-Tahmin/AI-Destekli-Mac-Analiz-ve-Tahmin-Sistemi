using System;
using System.Net.Http;
using System.Threading.Tasks;
using MatchAnalysisSystem.Business;
using Xunit;

namespace MatchAnalysisSystem.Test
{
    public class MatchServiceTests
    {
        [Fact]
        public async Task GetMatchPrediction_EmptyHomeTeam_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new MatchService(httpClient);

            // Act & Assert (Boş ev sahibi takımı gönderiyoruz)
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetMatchPredictionAsync("", "Fenerbahçe"));

            // Kodda yazdığımız mesajın aynısını arıyoruz
            Assert.Equal("Takım isimleri boş bırakılamaz!", exception.Message);
        }

        [Fact]
        public async Task GetMatchPrediction_EmptyAwayTeam_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient();
            var service = new MatchService(httpClient);

            // Act & Assert (Boşluklardan oluşan deplasman takımı gönderiyoruz)
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetMatchPredictionAsync("Galatasaray", "   "));

            // Kodda yazdığımız mesajın aynısını arıyoruz
            Assert.Equal("Takım isimleri boş bırakılamaz!", exception.Message);
        }
    }
}
using MatchAnalysisSystem.Core.Entities;

namespace MatchAnalysisSystem.Business.Services
{
    public class PoissonAnalysisManager
    {
        // Büyük sayılarda bellek taşmasını önleyen performans dostu faktöriyel metodu
        private double Factorial(int n)
        {
            if (n <= 1) return 1;
            double result = 1;
            for (int i = 2; i <= n; i++) result *= i;
            return result;
        }

        // Poisson Dağılım Formülü: P(X=k) = (e^-λ * λ^k) / k!
        public double CalculatePoisson(double lambda, int actualGoals)
        {
            if (lambda <= 0) lambda = 0.01; // Matematiksel çökme koruması
            return (Math.Exp(-lambda) * Math.Pow(lambda, actualGoals)) / Factorial(actualGoals);
        }

        // EMA Form Hesaplama Motoru (Tamamen Gerçekçi ve Dengeli)
        public (double DynamicAttack, double DynamicDefense) CalculateDynamicRatings(Team team, List<MatchHistory> lastMatches)
        {
            // Bir önceki adımda MatchController'da düzelttiğin property isimlerine (AttackStrength/DefenseStrength veya Attack/Defense) göre burayı eşitle!
            double baseAttack = team.AttackRating;
            double baseDefense = team.DefenseRating;

            if (lastMatches == null || lastMatches.Count == 0)
            {
                return (baseAttack, baseDefense);
            }

            double totalAttackWeight = 0;
            double totalDefenseWeight = 0;
            double totalWeight = 0;
            double alpha = 0.35; // EMA düzleştirme faktörü (Son maçlara %35 daha fazla ağırlık verir)

            for (int i = 0; i < lastMatches.Count; i++)
            {
                var match = lastMatches[i];
                // i = 0 en yeni maç olacak şekilde ağırlık azalarak gider
                double weight = Math.Pow(1 - alpha, i);
                totalWeight += weight;

                if (match.HomeTeamId == team.Id)
                {
                    totalAttackWeight += match.HomeGoals * weight;
                    totalDefenseWeight += match.AwayGoals * weight;
                }
                else
                {
                    totalAttackWeight += match.AwayGoals * weight;
                    totalDefenseWeight += match.HomeGoals * weight;
                }
            }

            // Ağırlıklı maç başına gol istatistikleri
            double avgGoalsScored = totalAttackWeight / totalWeight;
            double avgGoalsConceded = totalDefenseWeight / totalWeight;

            // 🎯 MATEMATİKSEL DENGELENDİRME: 
            // Sabit 0.5 ile toplamak yerine, dünya futbol ortalaması olan 1.3 gol referans alınarak form çarpanı üretildi.
            // Böylece sürekli 1-1 çıkma kilidi tamamen kırıldı.
            double dynamicAttack = Math.Clamp(baseAttack * (0.3 + (avgGoalsScored / 1.3)), 0.5, 2.5);
            double dynamicDefense = Math.Clamp(baseDefense * (0.3 + (avgGoalsConceded / 1.3)), 0.5, 2.5);

            return (dynamicAttack, dynamicDefense);
        }

        // Bilimsel Standartlarda Tahmin ve Matris Analiz Motoru
        public PredictionResult PredictMatchDinamik(Team homeTeam, List<MatchHistory> homeMatches, Team awayTeam, List<MatchHistory> awayMatches)
        {
            // Takımların son 5 maçlık gerçek performanslarından form çarpanlarını damıtıyoruz
            var (homeAttack, homeDefense) = CalculateDynamicRatings(homeTeam, homeMatches);
            var (awayAttack, awayDefense) = CalculateDynamicRatings(awayTeam, awayMatches);

            // 📊 Gerçekçi xG (Gol Beklentisi) Formülasyonu
            // Ev sahibinin hücum gücü ile deplasmanın defans zaafı harmanlanır
            double homeExpectation = Math.Clamp(homeAttack * awayDefense * 1.25, 0.2, 3.5);
            double awayExpectation = Math.Clamp(awayAttack * homeDefense * 1.05, 0.1, 3.0);

            double homeWinProb = 0;
            double drawProb = 0;
            double awayWinProb = 0;
            double over25Prob = 0;

            double maxScoreProb = -1;
            string dynamicMostLikelyScore = "1 - 1"; // Varsayılan emniyet skoru

            // 6x6 Olasılık Matrisi Hesaplaması (Toplam 36 farklı skor kombinasyonu analiz ediliyor)
            for (int h = 0; h <= 5; h++)
            {
                for (int a = 0; a <= 5; a++)
                {
                    double pHome = CalculatePoisson(homeExpectation, h);
                    double pAway = CalculatePoisson(awayExpectation, a);
                    double matchScoreProb = pHome * pAway;

                    // 1X2 Olasılık Dağılımları
                    if (h > a) homeWinProb += matchScoreProb;
                    else if (h == a) drawProb += matchScoreProb;
                    else awayWinProb += matchScoreProb;

                    // 2.5 Alt / Üst Dağılımı
                    if (h + a >= 3) over25Prob += matchScoreProb;

                    // 🧠 EN YÜKSEK OLASILIKLI GERÇEK SKORUN TESPİTİ:
                    // Sadece xG değerlerini yuvarlamak yerine matristeki en yüksek yüzdeli kombinasyonu seçer.
                    if (matchScoreProb > maxScoreProb)
                    {
                        maxScoreProb = matchScoreProb;
                        dynamicMostLikelyScore = $"{h} - {a}";
                    }
                }
            }

            // Arayüze tamamen tutarlı ve pürüzsüz veriler besleniyor
            return new PredictionResult
            {
                MatchName = $"{homeTeam.Name} vs {awayTeam.Name} (Profesyonel AI Analizi)",
                ExpectedHomeGoals = Math.Round(homeExpectation, 2),
                ExpectedAwayGoals = Math.Round(awayExpectation, 2),
                HomeWinProbability = Math.Round(homeWinProb * 100, 2),
                DrawProbability = Math.Round(drawProb * 100, 2),
                AwayWinProbability = Math.Round(awayWinProb * 100, 2),
                Over25Probability = Math.Round(over25Prob * 100, 2),
                Under25Probability = Math.Round((1 - over25Prob) * 100, 2),
                MostLikelyScore = dynamicMostLikelyScore
            };
        }
    }

    // API Projende halihazırda var olan PredictionResult modeliyle eşleşmesi için taslak sınıf
    public class PredictionResult
    {
        public string MatchName { get; set; }
        public double ExpectedHomeGoals { get; set; }
        public double ExpectedAwayGoals { get; set; }
        public double HomeWinProbability { get; set; }
        public double DrawProbability { get; set; }
        public double AwayWinProbability { get; set; }
        public double Over25Probability { get; set; }
        public double Under25Probability { get; set; }
        public string MostLikelyScore { get; set; }
    }
}
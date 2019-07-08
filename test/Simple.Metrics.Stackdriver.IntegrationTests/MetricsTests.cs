using System;
using System.Threading;
using Xunit;

namespace Simple.Metrics.Stackdriver.IntegrationTests
{
    public class MetricsTests
    {
        [Fact(Skip = "Integration test")]
        public void ExportsStackdriverMetrics()
        {
            var metrics = Metrics.Create(
                new MetricsOptions(
                    "sentiment-analysis-161716",
                    exportInterval: TimeSpan.FromSeconds(5)));

            var rand = new Random(0);

            for (var i = 0; i < 10000; i++)
            {
                metrics.Measure(
                    "web/request_latency_ms",
                    GetNormalDistributionValue(rand),
                    ("pod_name", "axd_3802"));
            }

            Metrics.StartExporting();

            // Wait a few seconds for export to finish.
            Thread.Sleep(10000);
        }

        private static int GetNormalDistributionValue(Random rand)
        {
            const double mean = 80;
            const double stdDev = 8;

            var u1 = 1.0 - rand.NextDouble();
            var u2 = 1.0 - rand.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return (int)(mean + stdDev * randStdNormal);
        }
    }
}
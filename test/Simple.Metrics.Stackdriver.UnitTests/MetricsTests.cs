using System;
using Xunit;

namespace Simple.Metrics.Stackdriver.UnitTests
{
    public class MetricsTests
    {
        [Fact]
        public void Metrics_CreateWithoutOptions_ProducesMetricRecorder()
        {
            var metric = Metrics.Create("test-project");
            metric.Measure("request_latency_ms", 100);
        }

        [Fact]
        public void Metrics_CreateWithOptions_ProducesMetricRecorder()
        {
            var metric = Metrics.Create(
                new MetricsOptions("test-project", exportInterval: TimeSpan.FromSeconds(10)));
            metric.Measure("request_latency_ms", 100);
        }

        [Fact]
        public void Metrics_StartExporting()
        {
            Metrics.StartExporting();
        }

        [Fact]
        public void Metrics_StopExporting()
        {
            Metrics.StopExporting();
        }
    }
}
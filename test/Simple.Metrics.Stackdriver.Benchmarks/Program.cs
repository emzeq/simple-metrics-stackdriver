using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Google.Cloud.Monitoring.V3;
using Simple.Metrics.Stackdriver;
using Microsoft.Extensions.Logging.Abstractions;

namespace Simple.Metrics.Stackdriver.Benchmarks
{
    public class MetricRecorderBenchmark
    {
        private readonly MetricsRecorder _metrics = new MetricsRecorder(new NullLogger<MetricsRecorder>());

        public MetricRecorderBenchmark()
        {
            _metrics.Set(
                "cpu_utilization",
                20,
                ("machine_name", "web_node_1"));
            _metrics.Set(
                "memory_utilization",
                55,
                ("machine_name", "web_node_1"));
        }

        [Benchmark]
        public void DoWork() => _metrics.Set(
            "request_latency_ms",
            500,
            ("action", "get_user")
        );

        private class NullExporter : IMetricsExporter
        {
            public Task ExportAsync(IEnumerable<TimeSeries> _)
            {
                return Task.CompletedTask;
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MetricRecorderBenchmark>();
        }
    }
}
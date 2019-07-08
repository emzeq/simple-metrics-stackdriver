using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using static Google.Api.MetricDescriptor.Types;

namespace Simple.Metrics.Stackdriver.UnitTests
{
    public class MetricRecorderFlushTests
    {
        private readonly FakeMetricExporter _fakeExporter = new FakeMetricExporter();
        private readonly MetricsRecorder _metrics = new MetricsRecorder(new NullLogger<MetricsRecorder>());

        [Fact]
        public async Task Recorder_Flush_GroupsNameAndLabels()
        {
            _metrics.Set("request_latency_ms", 100, ("action", "get_user"));
            _metrics.Set("disk_utilized_bytes", 20.5, ("disk", "/dev/sda1"));
            _metrics.Set("request_latency_ms", 300, ("action", "get_user"));
            _metrics.Set("request_latency_ms", 400, ("action", "get_attempts"));

            await _metrics.FlushAsync(_fakeExporter);

            _fakeExporter.AssertTimeSeriesCount(3);
            _fakeExporter.AssertExportedInt64(
                "request_latency_ms",
                MetricKind.Gauge,
                300,
                ("action", "get_user"));
            _fakeExporter.AssertExportedDouble(
                "disk_utilized_bytes",
                MetricKind.Gauge,
                20.5,
                ("disk", "/dev/sda1"));
            _fakeExporter.AssertExportedInt64(
                "request_latency_ms",
                MetricKind.Gauge,
                400,
                ("action", "get_attempts"));
        }

        [Fact]
        public async Task Recorder_Flush_ClearsRecordedPoints()
        {
            _metrics.Set("request_latency_ms", 100, ("action", "get_user"));

            await _metrics.FlushAsync(_fakeExporter);
            _fakeExporter.AssertTimeSeriesCount(1);
            await _metrics.FlushAsync(_fakeExporter);
            _fakeExporter.AssertTimeSeriesCount(0);
        }

        [Fact]
        public async Task Recorder_Set_UsesLastValue()
        {
            _metrics.Set("cpu_utilization", 25);
            _metrics.Set("cpu_utilization", 42);

            await _metrics.FlushAsync(_fakeExporter);

            _fakeExporter.AssertTimeSeriesCount(1);
            _fakeExporter.AssertExportedInt64(
                "cpu_utilization",
                MetricKind.Gauge,
                42);
        }

        [Fact]
        public async Task Recorder_Set_SupportsAllTypes()
        {
            _metrics.Set("cpu_utilization", 25);
            _metrics.Set("disk_read_bytes", 42L);
            _metrics.Set("mem_used_bytes", 4232.8F);
            _metrics.Set("mem_available_bytes", 4238.8D);
            _metrics.Set("machine_state", "OFF");

            await _metrics.FlushAsync(_fakeExporter);

            _fakeExporter.AssertTimeSeriesCount(5);
            _fakeExporter.AssertExportedInt64(
                "cpu_utilization",
                MetricKind.Gauge,
                25);
            _fakeExporter.AssertExportedInt64(
                "disk_read_bytes",
                MetricKind.Gauge,
                42);
            _fakeExporter.AssertExportedDouble(
                "mem_used_bytes",
                MetricKind.Gauge,
                4232.8);
            _fakeExporter.AssertExportedDouble(
                "mem_available_bytes",
                MetricKind.Gauge,
                4238.8);
            _fakeExporter.AssertExportedString(
                "machine_state",
                MetricKind.Gauge,
                "OFF");
        }

        [Fact]
        public async Task Recorder_Increment_SupportsAllTypes()
        {
            _metrics.Increment("total_requests", 25);
            _metrics.Increment("total_errors", 42L);
            _metrics.Increment("gpu_1_temp_c", 25.4F);
            _metrics.Increment("gpu_2_temp_c", 28.8D);

            await _metrics.FlushAsync(_fakeExporter);

            _fakeExporter.AssertTimeSeriesCount(4);
            _fakeExporter.AssertExportedInt64(
                "total_requests",
                MetricKind.Gauge,
                25);
            _fakeExporter.AssertExportedInt64(
                "total_errors",
                MetricKind.Gauge,
                42);
            _fakeExporter.AssertExportedDouble(
                "gpu_1_temp_c",
                MetricKind.Gauge,
                25.4);
            _fakeExporter.AssertExportedDouble(
                "gpu_2_temp_c",
                MetricKind.Gauge,
                28.8);
        }

        [Fact]
        public async Task Recorder_Increment_AddsValues()
        {
            _metrics.Increment("total_requests", 100, ("action", "get_user"));
            _metrics.Increment("total_requests", 50, ("action", "get_user"));

            await _metrics.FlushAsync(_fakeExporter);

            _fakeExporter.AssertTimeSeriesCount(1);
            _fakeExporter.AssertExportedInt64(
                "total_requests",
                MetricKind.Gauge,
                150,
                ("action", "get_user"));
        }

        [Fact]
        public async Task Recorder_Measure_CreatesDistribution()
        {
            _metrics.Measure("request_latency_ms", 100);
            _metrics.Measure("request_latency_ms", 50);
            _metrics.Measure("request_latency_ms", 1000);
            _metrics.Measure("request_latency_ms", 500);

            await _metrics.FlushAsync(_fakeExporter);

            _fakeExporter.AssertTimeSeriesCount(1);
            _fakeExporter.AssertExportedDistribution(
                "request_latency_ms",
                MetricKind.Gauge,
                count: 4,
                mean: 412.5,
                bucketCount: 6, // Num of measures + 2 overflows
                sumOfSquaredDeviation: 581875);
        }

        [Theory]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("/leading/forward_slash", typeof(ArgumentException))]
        public void Recorder_Record_ValidatesName(string name, Type expectedException)
        {
            Assert.Throws(expectedException, () => _metrics.Set(name, 10));
        }
    }
}
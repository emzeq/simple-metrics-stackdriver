using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Monitoring.V3;
using Xunit;
using static Google.Api.MetricDescriptor.Types;

namespace Simple.Metrics.Stackdriver.UnitTests
{
    public class FakeMetricExporter : IMetricsExporter
    {
        private IEnumerable<TimeSeries> _timeSeries;

        public void AssertTimeSeriesCount(int expected) => Assert.Equal(expected, _timeSeries.Count());

        public Task ExportAsync(IEnumerable<TimeSeries> timeSeries)
        {
            _timeSeries = timeSeries;
            return Task.CompletedTask;
        }

        internal void AssertExportedInt64(
            string name,
            MetricKind kind,
            long value,
            params (string, string)[] labels)
        {
            var typedValue = AssertMatchingTimeSeries(name, kind, labels);
            Assert.Equal(value, typedValue.Int64Value);
        }

        internal void AssertExportedDouble(
            string name,
            MetricKind kind,
            double value,
            params (string, string)[] labels)
        {
            var typedValue = AssertMatchingTimeSeries(name, kind, labels);
            Assert.Equal(value, typedValue.DoubleValue, precision: 3);
        }

        internal void AssertExportedString(
            string name,
            MetricKind kind,
            string value,
            params (string, string)[] labels)
        {
            var typedValue = AssertMatchingTimeSeries(name, kind, labels);
            Assert.Equal(value, typedValue.StringValue);
        }

        internal void AssertExportedDistribution(
            string name,
            MetricKind kind,
            int count,
            double mean,
            int bucketCount,
            int sumOfSquaredDeviation,
            params (string, string)[] labels)
        {
            var typedValue = AssertMatchingTimeSeries(name, kind, labels);
            Assert.NotNull(typedValue.DistributionValue);
            Assert.Equal(count, typedValue.DistributionValue.Count);
            Assert.Equal(mean, typedValue.DistributionValue.Mean);
            Assert.Equal(bucketCount, typedValue.DistributionValue.BucketCounts.Count);
            Assert.Equal(sumOfSquaredDeviation, typedValue.DistributionValue.SumOfSquaredDeviation);
        }

        private TypedValue AssertMatchingTimeSeries(string name, MetricKind kind, (string, string)[] labels)
        {
            var match = _timeSeries
                .SingleOrDefault(ts => ts.Metric.Type == "custom.googleapis.com/" + name
                                       && ts.MetricKind == kind
                                       && ts.Metric.Labels.Select(l => (l.Key, l.Value)).SequenceEqual(labels));

            Assert.True(match != null, $"Did not find a match for {name}.");
            Assert.True(match.Points != null, $"Did not find a time series point for {name}.");
            Assert.True(match.Points.Count() == 1, $"Time series {name} did not contain a single point.");
            Assert.True(match.Points[0].Value != null, $"Did not find a time series value for {name}.");

            return match.Points[0].Value;
        }
    }
}
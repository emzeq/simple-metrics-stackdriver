using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Api;
using Google.Cloud.Monitoring.V3;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Simple.Metrics.Stackdriver.Operations;
using Simple.Metrics.Stackdriver.Values;
using DoubleValue = Simple.Metrics.Stackdriver.Values.DoubleValue;
using StringValue = Simple.Metrics.Stackdriver.Values.StringValue;

namespace Simple.Metrics.Stackdriver
{
    internal class MetricsRecorder : IMetricsRecorder
    {
        private readonly ILogger<MetricsRecorder> _logger;

        private ConcurrentDictionary<string, TimeSeriesData> _values
            = new ConcurrentDictionary<string, TimeSeriesData>();

        public MetricsRecorder(ILogger<MetricsRecorder> logger)
        {
            _logger = logger;
        }

        public void Set(
            string name,
            int value,
            params (string Key, string Value)[] labels)
        {
            var point = new LongValue(value);
            var operation = new SetOperation();
            Record(name, operation, point, labels);
        }

        public void Set(
            string name,
            long value,
            params (string Key, string Value)[] labels)
        {
            var point = new LongValue(value);
            var operation = new SetOperation();
            Record(name, operation, point, labels);
        }

        public void Set(
            string name,
            float value,
            params (string Key, string Value)[] labels)
        {
            var point = new DoubleValue(value);
            var operation = new SetOperation();
            Record(name, operation, point, labels);
        }

        public void Set(
            string name,
            double value,
            params (string Key, string Value)[] labels)
        {
            var point = new DoubleValue(value);
            var operation = new SetOperation();
            Record(name, operation, point, labels);
        }

        public void Set(
            string name,
            string value,
            params (string Key, string Value)[] labels)
        {
            var point = new StringValue(value);
            var operation = new SetOperation();
            Record(name, operation, point, labels);
        }

        public void Increment(
            string name,
            int value,
            params (string Key, string Value)[] labels)
        {
            var point = new LongValue(value);
            var operation = new IncrementOperation();
            Record(name, operation, point, labels);
        }

        public void Increment(
            string name,
            long value,
            params (string Key, string Value)[] labels)
        {
            var point = new LongValue(value);
            var operation = new IncrementOperation();
            Record(name, operation, point, labels);
        }

        public void Increment(
            string name,
            float value,
            params (string Key, string Value)[] labels)
        {
            var point = new DoubleValue(value);
            var operation = new IncrementOperation();
            Record(name, operation, point, labels);
        }

        public void Increment(
            string name,
            double value,
            params (string Key, string Value)[] labels)
        {
            var point = new DoubleValue(value);
            var operation = new IncrementOperation();
            Record(name, operation, point, labels);
        }

        public void Measure(
            string name,
            int value,
            params (string Key, string Value)[] labels)
        {
            var point = new DistributionValue(value);
            var operation = new MeasureOperation();
            Record(name, operation, point, labels);
        }

        private void Record(
            string name,
            IOperation operation,
            IValue value,
            (string Key, string Value)[] labels)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name[0] == '/')
            {
                throw new ArgumentException("Metric name cannot start with a forward slash.", nameof(name));
            }

            var timeSeries = _values.GetOrAdd(
                BuildTimeSeriesKey(name, labels),
                _ => new TimeSeriesData(name, labels, operation, value.Default));

            lock (timeSeries)
            {
                timeSeries.Value = operation.Apply(timeSeries.Value, value);
            }

            _logger.LogTrace("Recorded point for metric {Metric}", name);
        }

        public async Task FlushAsync(IMetricsExporter exporter)
        {
            var flushing = Interlocked.Exchange(
                ref _values,
                new ConcurrentDictionary<string, TimeSeriesData>());
            var timeSeries = flushing.Values.Select(v => v.ToTimeSeries()).ToList();
            await exporter.ExportAsync(timeSeries);
        }

        private static string BuildTimeSeriesKey(string name, (string Key, string Value)[] labels)
        {
            var sb = new StringBuilder();
            sb.Append(name);

            foreach (var (key, value) in labels)
            {
                sb.Append(":");
                sb.Append(key);
                sb.Append("-");
                sb.Append(value);
            }

            return sb.ToString();
        }

        private static Timestamp GetCurrentTimestamp()
        {
            var millis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new Timestamp
            {
                Seconds = millis / 1000,
                Nanos = (int)((millis % 1000) * 1000000)
            };
        }

        private class TimeSeriesData
        {
            private readonly string _name;
            private readonly (string Key, string Value)[] _labels;
            private readonly IOperation _operation;

            public IValue Value { get; set; }

            public TimeSeriesData(
                string name,
                (string Key, string Value)[] labels,
                IOperation operation,
                IValue defaultValue)
            {
                _name = name;
                _labels = labels;
                _operation = operation;
                Value = defaultValue;
            }

            public TimeSeries ToTimeSeries()
                => new TimeSeries
                {
                    Metric = new Metric
                    {
                        Type = "custom.googleapis.com/" + _name,
                        Labels = { _labels.ToDictionary(a => a.Key, a => a.Value) }
                    },
                    MetricKind = MetricDescriptor.Types.MetricKind.Gauge,
                    Points =
                    {
                        new Point
                        {
                            Interval = new TimeInterval { EndTime = GetCurrentTimestamp() },
                            Value = Value.CreateTypedValue()
                        }
                    }
                };
        };
    }
}
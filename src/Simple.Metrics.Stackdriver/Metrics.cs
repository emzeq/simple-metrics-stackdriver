using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;

namespace Simple.Metrics.Stackdriver
{
    public static class Metrics
    {
        private static bool _exportingStarted = false;
        private static readonly object Locker = new object();
        private static readonly Dictionary<string, (MetricsRecorder Recorder, MetricsExporter Exporter)> MetricTarget
            = new Dictionary<string, (MetricsRecorder, MetricsExporter)>();

        public static IMetricsRecorder Create(string projectId) => Create(new MetricsOptions(projectId));

        public static IMetricsRecorder Create(MetricsOptions options)
        {
            lock (Locker)
            {
                var projectId = options.ProjectId;

                if (MetricTarget.TryGetValue(projectId, out var metricTarget))
                {
                    return metricTarget.Recorder;
                }

                var recorder = new MetricsRecorder(new NullLogger<MetricsRecorder>());
                var exporter = new MetricsExporter(options, recorder, new NullLogger<MetricsExporter>());

                if (_exportingStarted)
                {
                    exporter.Start();
                }

                MetricTarget.Add(projectId, (recorder, exporter));

                return recorder;
            }
        }

        public static void StartExporting(CancellationToken? cancellationToken = null)
        {
            lock (Locker)
            {
                _exportingStarted = true;

                foreach (var (_, exporter) in MetricTarget.Values)
                {
                    exporter.Start(cancellationToken);
                }
            }
        }

        public static void StopExporting()
        {
            lock (Locker)
            {
                _exportingStarted = false;

                foreach (var (_, exporter) in MetricTarget.Values)
                {
                    exporter.Stop();
                }
            }
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.Grpc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Monitoring.V3;
using Grpc.Auth;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Simple.Metrics.Stackdriver
{
    public class MetricsExporter : IMetricsExporter
    {
        private readonly MetricsOptions _options;
        private readonly MetricServiceClient _client;
        private readonly IMetricsRecorder _recorder;
        private readonly ILogger<MetricsExporter> _logger;
        private readonly CancellationTokenSource _internalCancellationSource = new CancellationTokenSource();
        private CancellationToken _cancellationToken;

        public MetricsExporter(
            MetricsOptions options,
            IMetricsRecorder recorder,
            ILogger<MetricsExporter> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var credential = string.IsNullOrEmpty(options.JsonPath)
                ? GoogleCredential.GetApplicationDefault()
                : GoogleCredential
                    .FromFile(options.JsonPath)
                    .CreateScoped(MetricServiceClient.DefaultScopes);

            var channel = new Channel(
                MetricServiceClient.DefaultEndpoint.Host,
                MetricServiceClient.DefaultEndpoint.Port,
                credential.ToChannelCredentials());

            _client = MetricServiceClient.Create(channel);
        }

        public async Task ExportAsync(IEnumerable<TimeSeries> timeSeries)
        {
            if (!timeSeries.Any())
            {
                return;
            }

            var callTiming = CallTiming.FromTimeout(_options.ExportInterval);

            var callSettings = new CallSettings(
                _cancellationToken,
                null,
                callTiming,
                null,
                null,
                null);

            await _client.CreateTimeSeriesAsync(
                new ProjectName(_options.ProjectId),
                timeSeries,
                callSettings);
        }

        public void Start(CancellationToken? cancellationToken = null)
        {
            _logger.LogInformation("Starting Stackdriver metric exporter");

            _cancellationToken = cancellationToken != null
                ? CancellationTokenSource
                    .CreateLinkedTokenSource(
                        cancellationToken.Value,
                        _internalCancellationSource.Token)
                    .Token
                : _internalCancellationSource.Token;

            Task.Run(DoWorkAsync);
        }

        public void Stop()
        {
            _logger.LogInformation("Stopping Stackdriver metric exporter");
            _internalCancellationSource.Cancel();
        }

        private async Task DoWorkAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Exporting metrics to Stackdriver...");

                    var stopwatch = Stopwatch.StartNew();
                    await _recorder.FlushAsync(this);
                    stopwatch.Stop();

                    _logger.LogDebug("Exporting metrics to Stackdriver finished");

                    var delay = _options.ExportInterval - stopwatch.Elapsed;

                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay, _cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to export metrics to Stackdriver");

                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(_options.ExportInterval, _cancellationToken);
                    }
                }
            }

            _logger.LogInformation("Stackdriver metric exporter stopped");
        }
    }
}
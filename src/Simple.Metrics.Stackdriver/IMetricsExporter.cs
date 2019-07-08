using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Monitoring.V3;

namespace Simple.Metrics.Stackdriver
{
    public interface IMetricsExporter
    {
        Task ExportAsync(IEnumerable<TimeSeries> timeSeries);
    }
}
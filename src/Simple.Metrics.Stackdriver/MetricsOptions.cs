using System;

namespace Simple.Metrics.Stackdriver
{
    public class MetricsOptions
    {
        public string ProjectId { get; private set; }
        public string JsonPath { get; private set; }
        public TimeSpan ExportInterval { get; private set; }

        public MetricsOptions(
            string projectId,
            string jsonPath = null,
            TimeSpan? exportInterval = null)
        {
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            ProjectId = projectId;
            JsonPath = jsonPath;
            ExportInterval = exportInterval ?? TimeSpan.FromMinutes(1);
        }
    }
}
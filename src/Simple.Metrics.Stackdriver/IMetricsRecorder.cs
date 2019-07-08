using System.Threading.Tasks;

namespace Simple.Metrics.Stackdriver
{
    public interface IMetricsRecorder
    {
        void Set(
            string name,
            int value,
            params (string Key, string Value)[] labels);

        void Set(
            string name,
            long value,
            params (string Key, string Value)[] labels);

        void Set(
            string name,
            float value,
            params (string Key, string Value)[] labels);

        void Set(
            string name,
            double value,
            params (string Key, string Value)[] labels);

        void Set(
            string name,
            string value,
            params (string Key, string Value)[] labels);

        void Increment(
            string name,
            int value,
            params (string Key, string Value)[] labels);

        void Increment(
            string name,
            long value,
            params (string Key, string Value)[] labels);

        void Measure(
            string name,
            int value,
            params (string Key, string Value)[] labels);

        Task FlushAsync(IMetricsExporter exporter);
    }
}
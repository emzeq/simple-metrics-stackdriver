using Simple.Metrics.Stackdriver.Values;
using static Google.Api.MetricDescriptor.Types;

namespace Simple.Metrics.Stackdriver.Operations
{
    internal struct MeasureOperation : IOperation
    {
        public IValue Apply(IValue previousValue, IValue newValue) => previousValue.Measure(newValue);
    }
}
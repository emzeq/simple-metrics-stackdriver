using Simple.Metrics.Stackdriver.Values;
using static Google.Api.MetricDescriptor.Types;

namespace Simple.Metrics.Stackdriver.Operations
{
    internal interface IOperation
    {
        IValue Apply(IValue previousValue, IValue currentValue);
    }
}
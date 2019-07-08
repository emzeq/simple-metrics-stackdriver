using Simple.Metrics.Stackdriver.Values;

namespace Simple.Metrics.Stackdriver.Operations
{
    internal struct IncrementOperation : IOperation
    {
        public IValue Apply(IValue previousValue, IValue newValue) => previousValue.Increment(newValue);
    }
}
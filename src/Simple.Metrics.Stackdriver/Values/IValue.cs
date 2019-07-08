using Google.Cloud.Monitoring.V3;

namespace Simple.Metrics.Stackdriver.Values
{
    internal interface IValue
    {
        IValue Default { get; }
        IValue Increment(IValue value);
        IValue Measure(IValue value);
        TypedValue CreateTypedValue();
    }
}
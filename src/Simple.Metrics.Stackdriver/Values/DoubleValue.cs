using System;
using Google.Cloud.Monitoring.V3;

namespace Simple.Metrics.Stackdriver.Values
{
    internal struct DoubleValue : IValue
    {
        public IValue Default => new DoubleValue(0);
        private readonly double _value;

        public DoubleValue(double value)
        {
            _value = value;
        }

        public IValue Increment(IValue value) => new DoubleValue(_value + ((DoubleValue)value)._value);

        public IValue Measure(IValue typedValue) => throw new NotSupportedException();

        public TypedValue CreateTypedValue() => new TypedValue { DoubleValue = _value };
    }
}
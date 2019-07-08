using System;
using Google.Cloud.Monitoring.V3;

namespace Simple.Metrics.Stackdriver.Values
{
    internal struct LongValue : IValue
    {
        public IValue Default => new LongValue(0);
        private readonly long _value;

        public LongValue(long value)
        {
            _value = value;
        }

        public IValue Increment(IValue value) => new LongValue(_value + ((LongValue)value)._value);

        public IValue Measure(IValue value) => throw new NotImplementedException();

        public TypedValue CreateTypedValue() => new TypedValue { Int64Value = _value };
    }
}
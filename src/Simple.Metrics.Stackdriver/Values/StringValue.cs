using System;
using Google.Cloud.Monitoring.V3;
using HdrHistogram;

namespace Simple.Metrics.Stackdriver.Values
{
    internal struct StringValue : IValue
    {
        public IValue Default => new StringValue(string.Empty);
        private readonly string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public IValue Increment(IValue value) => throw new NotImplementedException();

        public IValue Measure(IValue value) => throw new NotImplementedException();

        public TypedValue CreateTypedValue() => new TypedValue { StringValue = _value };
    }
}
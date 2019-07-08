using System;
using System.Linq;
using Google.Api;
using Google.Cloud.Monitoring.V3;
using HdrHistogram;
using static Google.Api.Distribution.Types;
using static Google.Api.Distribution.Types.BucketOptions.Types;

namespace Simple.Metrics.Stackdriver.Values
{
    internal struct DistributionValue : IValue
    {
        private readonly HistogramBase _histogram;
        private readonly long _value;

        public IValue Default => new DistributionValue(
            0,
            new LongHistogram(highestTrackableValue: TimeStamp.Hours(1), numberOfSignificantValueDigits: 3));

        private DistributionValue(long value, HistogramBase histogram)
        {
            _histogram = histogram;
            _value = value;
        }

        public DistributionValue(long value)
        {
            _histogram = null;
            _value = value;
        }

        public IValue Increment(IValue value) => throw new NotSupportedException();

        public IValue Measure(IValue value)
        {
            var distributionValue = (DistributionValue)value;
            _histogram.RecordValue(distributionValue._value);
            return new DistributionValue(0, _histogram);
        }

        public TypedValue CreateTypedValue()
        {
            var buckets = _histogram
                .Percentiles(3)
                .Select(v => new
                {
                    v.CountAddedInThisIterationStep,
                    v.ValueIteratedTo
                })
                .GroupBy(v => v.ValueIteratedTo)
                .Select(v => v.First())
                .ToList();

            var bucketCounts = buckets.Select(v => v.CountAddedInThisIterationStep).ToList();
            bucketCounts.Add(0);

            var bucketBounds = buckets.Select(v => (double)v.ValueIteratedTo).ToList();

            var hasZeroLowerBounds = bucketBounds[0] == 0;
            if (!hasZeroLowerBounds)
            {
                bucketBounds.Insert(0, 0);
                bucketCounts.Insert(0, 0);
            }

            return new TypedValue
            {
                DistributionValue = new Distribution
                {
                    BucketCounts = { bucketCounts },
                    BucketOptions = new BucketOptions
                    {
                        ExplicitBuckets = new Explicit { Bounds = { bucketBounds } }
                    },
                    Count = bucketCounts.Sum(),
                    Mean = _histogram.GetMean(),
                    SumOfSquaredDeviation = Math.Pow(_histogram.GetStdDeviation(), 2) * _histogram.TotalCount
                }
            };
        }
    }
}
using System.Linq;
using UnityEngine;

namespace Genetics.GeneSummarization
{
    public class ContinuousSummary : AbstractSummary
    {
        private float bucketSize;
        private float minValue;
        private float maxValue;

        public ContinuousSummary(
            float minValue,
            float maxValue,
            int bucketNumber)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            bucketSize = (maxValue - minValue) / bucketNumber;

            InitializeBuckets(
                Enumerable.Range(0, bucketNumber)
                .Select(x => $"{minValue + x * bucketSize} - {minValue + (x + 1) * bucketSize}")
                );
        }

        public override void ClassifyValue(float value)
        {
            var bucketIndex = Mathf.FloorToInt((value - minValue) / bucketSize);
            base.ClassifyValue(bucketIndex);
        }
    }
}

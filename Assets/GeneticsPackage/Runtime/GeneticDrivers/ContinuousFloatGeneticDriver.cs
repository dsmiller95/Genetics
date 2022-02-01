using Genetics.GeneSummarization;
using UnityEngine;

namespace Genetics.GeneticDrivers
{
    [CreateAssetMenu(fileName = "ContinuousFloatDriver", menuName = "Genetics/ContinuousFloatDriver", order = 10)]
    public class ContinuousFloatGeneticDriver : FloatGeneticDriver
    {
        public float minValue;
        public float maxValue;
        [Tooltip("when less that or equal to 0 will default to a bucket size close to 1")]
        public int summaryBucketCount = 0;
        public override string DescribeState(float state)
        {
            return $"{this.DriverName}: {NameState(state)}";
        }

        public override string NameState(float state)
        {
            return $"{state :F2}";
        }

        public override string DescribeRange(float min, float max)
        {
            return $"{this.DriverName} between {min} and {max}";
        }

        public override bool FallsInRange(float min, float max, float value)
        {
            return value >= min && value <= max;
        }

        public override AbstractSummary GetSummarizer()
        {
            return new ContinuousSummary(
                minValue,
                maxValue,
                summaryBucketCount <= 0 ? Mathf.FloorToInt(maxValue - minValue) : summaryBucketCount,
                this);
        }

        public override void SummarizeValue(AbstractSummary summarizer, CompiledGeneticDrivers valueSet)
        {
            if (valueSet == null || !valueSet.TryGetGeneticData(this, out var value))
            {
                summarizer.invalidClassifications++;
                return;
            }
            summarizer.ClassifyValue(value);
        }
    }
}

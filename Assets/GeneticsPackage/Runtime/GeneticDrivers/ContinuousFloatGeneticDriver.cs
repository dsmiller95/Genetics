using Genetics.GeneSummarization;
using UnityEngine;

namespace Genetics.GeneticDrivers
{
    [CreateAssetMenu(fileName = "ContinuousFloatDriver", menuName = "Genetics/ContinuousFloatDriver", order = 10)]
    public class ContinuousFloatGeneticDriver : FloatGeneticDriver
    {
        public float minValue;
        public float maxValue;
        public override string DescribeState(float state)
        {
            return this.DriverName + $": {state:F2}";
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
            return new ContinuousSummary(minValue, maxValue, Mathf.FloorToInt(maxValue - minValue));
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

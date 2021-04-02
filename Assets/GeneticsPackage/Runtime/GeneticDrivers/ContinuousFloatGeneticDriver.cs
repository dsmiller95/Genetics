using UnityEngine;

namespace Genetics.GeneticDrivers
{
    [CreateAssetMenu(fileName = "ContinuousFloatDriver", menuName = "Genetics/ContinuousFloatDriver", order = 10)]
    public class ContinuousFloatGeneticDriver : FloatGeneticDriver
    {
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
    }
}

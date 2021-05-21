using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.GeneSummarization
{
    public class ContinuousSummary : AbstractSummary
    {
        private float bucketSize;
        private float minValue;
        private float maxValue;

        public SortedList<float, int> sortedValues;

        public ContinuousSummary(
            float minValue,
            float maxValue,
            int bucketNumber)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            bucketSize = (maxValue - minValue) / bucketNumber;

            sortedValues = new SortedList<float, int>();
        }

        public override void ClassifyValue(float value)
        {
            if (sortedValues.ContainsKey(value))
            {
                sortedValues[value] += 1;
            }else
            {
                sortedValues[value] = 1;
            }
        }
    }
}

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

        public float[] RenderContinuousHistogram(int outputSpaceSize)
        {
            var outputSpace = new float[outputSpaceSize];

            var sampleSpaceToOutputSpace = outputSpaceSize / (maxValue - minValue);

            var outputBucketSize = bucketSize * sampleSpaceToOutputSpace;

            foreach (var sampleValues in sortedValues)
            {
                float intensity = sampleValues.Value;
                var centerInOutput = (sampleValues.Key - minValue) * sampleSpaceToOutputSpace;
                var min = Mathf.Max(0, (int)(centerInOutput - outputBucketSize / 2f));
                var max = Mathf.Min(outputSpace.Length, (int)(centerInOutput + outputBucketSize / 2f));

                for (int i = min; i < max; i++)
                {
                    outputSpace[i] += intensity;
                }
            }

            var maxIntensity = outputSpace.Max();

            return outputSpace.Select(x => x / maxIntensity).ToArray();
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

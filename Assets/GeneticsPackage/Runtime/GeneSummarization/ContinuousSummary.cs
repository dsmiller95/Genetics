using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.GeneSummarization
{
    public class ContinuousSummary : AbstractSummary
    {
        private float bucketSize;
        public float minValue;
        public float maxValue;

        public SortedList<float, int> sortedValues;

        public ContinuousSummary(
            float minValue,
            float maxValue,
            int bucketNumber,
            GeneticDriver source): base(source)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            bucketSize = (maxValue - minValue) / bucketNumber;

            sortedValues = new SortedList<float, int>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputSpaceSize">the length of the output array samples</param>
        /// <param name="sampleIntensityCurve">a function which takes in a number from 0 to 1, and returns a remapped intensity value based on distance from the value</param>
        /// <returns></returns>
        public float[] RenderContinuousHistogram(
            int outputSpaceSize,
            Func<float, float> sampleIntensityCurve)
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
                    var intensityCurvePosition = Mathf.Abs(i - centerInOutput) / (outputBucketSize / 2f);
                    if(intensityCurvePosition > 1 || intensityCurvePosition < 0)
                    {
                        continue;
                    }
                    var intensityHere = sampleIntensityCurve(intensityCurvePosition);
                    outputSpace[i] += intensity * intensityHere;
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
        public override string ToString()
        {
            return string.Join(
                ", ",
                RenderContinuousHistogram(10, x => 1)
                );
        }
    }
}

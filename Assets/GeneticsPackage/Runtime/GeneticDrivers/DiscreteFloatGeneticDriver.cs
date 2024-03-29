﻿using Genetics.GeneSummarization;
using System.Text;
using UnityEngine;

namespace Genetics.GeneticDrivers
{
    /// <summary>
    /// backed by a float, but classifies in discrete chunks. every value from x.0 to x.99999 is rounded to x;
    ///     that is, the number is rounded down to the closest int
    /// </summary>
    [CreateAssetMenu(fileName = "DiscreteFloatGeneticDriver", menuName = "Genetics/DiscreteFloatGeneticDriver", order = 10)]
    public class DiscreteFloatGeneticDriver : FloatGeneticDriver
    {
        public string[] possibleStates;
        public Color[] possibleColors;


        public override string DescribeState(float state)
        {
            var stateAsInt = Mathf.FloorToInt(state);

            return $"{this.DriverName}: {possibleStates[stateAsInt]}";
        }

        public override string NameState(float state)
        {
            var stateAsInt = Mathf.FloorToInt(state);
            return possibleStates[stateAsInt];
        }


        public override string DescribeRange(float min, float max)
        {
            var rangeClause = this.RangeClause(MinInt(min), MaxInt(max));

            return $"{this.DriverName}: {rangeClause}";
        }
        public override bool CompareRangeAsIntegers()
        {
            return true;
        }
        private int MinInt(float min) => Mathf.CeilToInt(min);
        private int MaxInt(float max) => Mathf.FloorToInt(max);

        private string RangeClause(int min, int max)
        {
            if (min == max)
            {
                return possibleStates[max];
            }
            if (max - min == 1)
            {
                return possibleStates[min] + " or " + possibleStates[max];
            }

            var result = new StringBuilder();

            for (int i = min; i < max; i++)
            {
                result.Append(possibleStates[i]);
                result.Append(", ");
            }
            result.Append("or ");
            result.Append(possibleStates[max]);
            return result.ToString();
        }

        public override AbstractSummary GetSummarizer()
        {
            return new DiscretSummary(
                possibleStates,
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Genetics.Genes;
using Genetics.ParameterizedGenomeGenerator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Genetics
{
    public class FloatGeneticTargetGeneratorTests
    {
        [Test]
        public void NeverGeneratesOutOfBounds()
        {
            var generator = new FloatGeneticTargetGenerator
            {
                absoluteMin = 1f,
                absoluteMax = 5f,
                rangeMin = 1f,
                rangeMax = 3f
            };

            var generatedResult = Enumerable.Range(0, 500).Select(x => generator.GenerateTarget());

            foreach (var result in generatedResult)
            {
                Assert.LessOrEqual(generator.absoluteMin, result.minValue, $"Expected generated min value {result.minValue} to be above absolute minimum {generator.absoluteMin}");
                Assert.GreaterOrEqual(generator.absoluteMax, result.maxValue, $"Expected generated max value {result.maxValue} to be below absolute maximum {generator.absoluteMax}");
            }
        }
        [Test]
        public void ValueRangeAlwaysWithinRange()
        {
            var generator = new FloatGeneticTargetGenerator
            {
                absoluteMin = 1f,
                absoluteMax = 5f,
                rangeMin = 1f,
                rangeMax = 3f
            };

            var generatedResult = Enumerable.Range(0, 500).Select(x => generator.GenerateTarget());

            foreach (var result in generatedResult)
            {
                var actualRange = result.maxValue - result.minValue;
                Assert.IsTrue(actualRange >= generator.rangeMin - 1e-5 && actualRange <= generator.rangeMax + 1e-5, $"Expected generated variance range {actualRange} to fall in [{generator.rangeMin}, {generator.rangeMax}]");
            }
        }

    }
}
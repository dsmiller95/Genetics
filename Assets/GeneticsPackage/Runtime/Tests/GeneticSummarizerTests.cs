using Genetics.GeneSummarization;
using Genetics.GeneticDrivers;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    public class GeneticSummarizerTests
    {
        [Test]
        public void SummarizesBooleanGeneDrivers()
        {
            currentGeneIndex = 0;
            var boolDrivers = Enumerable.Repeat(0, 3).Select(x => BoolDriver()).ToArray();

            var compiledValues = Enumerable.Repeat(0, 5)
                .Select(x => new CompiledGeneticDrivers())
                .ToArray();

            WriteValuesToGene(compiledValues, boolDrivers[0],
                ToBoolArray("00000"));
            WriteValuesToGene(compiledValues, boolDrivers[1],
                ToBoolArray("11111"));
            WriteValuesToGene(compiledValues, boolDrivers[2],
                ToBoolArray("10100"));

            var summaries = new GeneticDriverSummarySet(
                boolDrivers
                    .Cast<GeneticDriver>()
                    .ToArray(),
                compiledValues);

            AssertSequenceEqual(
                (summaries.summaries["0"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 5, 0 });
            AssertSequenceEqual(
                (summaries.summaries["1"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 0, 5 });
            AssertSequenceEqual(
                (summaries.summaries["2"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 3, 2 });
        }
        [Test]
        public void SummarizesBooleanGeneDriversWithUncertainty()
        {
            currentGeneIndex = 0;
            var boolDriver = BoolDriver();

            var compiledValues = Enumerable.Repeat(0, 5)
                .Select(x => new CompiledGeneticDrivers())
                .ToArray();

            compiledValues[0].SetGeneticDriverData(boolDriver, false);
            compiledValues[1].SetGeneticDriverData(boolDriver, true);
            compiledValues[2].SetGeneticDriverData(boolDriver, true);

            compiledValues[4] = null;


            var summaries = new GeneticDriverSummarySet(
                new[] { boolDriver },
                compiledValues);


            AssertSequenceEqual(
                (summaries.summaries["0"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 1, 2 });
            Assert.AreEqual(2, summaries.summaries["0"].invalidClassifications);
        }
        [Test]
        public void SummarizesDiscreteGeneDrivers()
        {
            currentGeneIndex = 0;
            var discreteDrivers = Enumerable.Repeat(0, 3).Select(x => DiscreteFloatDriver(4)).ToArray();

            var compiledValues = Enumerable.Repeat(0, 5)
                .Select(x => new CompiledGeneticDrivers())
                .ToArray();

            WriteValuesToGene(compiledValues, discreteDrivers[0],
                new[] { 0f, 0f, 0f, 0f, 0f });
            WriteValuesToGene(compiledValues, discreteDrivers[1],
                new[] { 0f, 1f, 2f, 3f, 3.9f });
            WriteValuesToGene(compiledValues, discreteDrivers[2],
                new[] { 1f, 2f, 2f, 1f, 2f });

            var summaries = new GeneticDriverSummarySet(
                discreteDrivers
                    .Cast<GeneticDriver>()
                    .ToArray(),
                compiledValues);

            AssertSequenceEqual(
                (summaries.summaries["0"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 5, 0, 0, 0 });
            AssertSequenceEqual(
                (summaries.summaries["1"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 1, 1, 1, 2 });
            AssertSequenceEqual(
                (summaries.summaries["2"] as DiscretSummary).allClassifications.Select(x => x.totalClassifications),
                new int[] { 0, 2, 3, 0 });
        }
        [Test]
        public void SummarizesFloatingGeneDrivers()
        {
            currentGeneIndex = 0;
            var floatingDrivers = Enumerable.Repeat(0, 3).Select(x => FloatDriver(1, 5)).ToArray();

            var compiledValues = Enumerable.Repeat(0, 5)
                .Select(x => new CompiledGeneticDrivers())
                .ToArray();

            WriteValuesToGene(compiledValues, floatingDrivers[0],
                new[] { 1f, 2.1f, 1.5f, 1.9f, 2.0f });
            WriteValuesToGene(compiledValues, floatingDrivers[1],
                new[] { 1f, 2f, 4.9f, 4f, 3f });
            WriteValuesToGene(compiledValues, floatingDrivers[2],
                new[] { 1f, 4f, 1f, 1f, 1f });

            var summaries = new GeneticDriverSummarySet(
                floatingDrivers
                    .Cast<GeneticDriver>()
                    .ToArray(),
                compiledValues);

            AssertSequenceEqual(
                (summaries.summaries["0"] as ContinuousSummary).sortedValues.Keys,
                new float[] { 1f, 1.5f, 1.9f, 2.0f, 2.1f });
            AssertSequenceEqual(
                (summaries.summaries["0"] as ContinuousSummary).sortedValues.Values,
                new int[] { 1, 1, 1, 1, 1 });

            AssertSequenceEqual(
                (summaries.summaries["1"] as ContinuousSummary).sortedValues.Keys,
                new float[] { 1f, 2f, 3f, 4f, 4.9f });
            AssertSequenceEqual(
                (summaries.summaries["1"] as ContinuousSummary).sortedValues.Values,
                new int[] { 1, 1, 1, 1, 1 });

            AssertSequenceEqual(
                (summaries.summaries["2"] as ContinuousSummary).sortedValues.Keys,
                new float[] { 1f, 4f });
            AssertSequenceEqual(
                (summaries.summaries["2"] as ContinuousSummary).sortedValues.Values,
                new int[] { 4, 1 });
        }

        private static void AssertSequenceEqual<T>(IEnumerable<T> actual, IEnumerable<T> expected)
        {
            if (!actual.SequenceEqual(expected))
            {
                var errorMessage = $"Expected ({string.Join(", ", actual)}) to equal ({string.Join(", ", expected)})";
                Assert.Fail(errorMessage);
            }
        }

        private void WriteValuesToGene<T>(CompiledGeneticDrivers[] compiledValues, GeneticDriver<T> driver, T[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                compiledValues[i].SetGeneticDriverData(driver, values[i]);
            }
        }
        private bool[] ToBoolArray(string input)
        {
            return input.Select(x => x == '1').ToArray();
        }


        private int currentGeneIndex;
        private BooleanGeneticDriver BoolDriver()
        {
            var boolDriver = ScriptableObject.CreateInstance<BooleanGeneticDriver>();
            boolDriver.DriverName = currentGeneIndex.ToString();
            boolDriver.myId = currentGeneIndex;
            currentGeneIndex++;
            return boolDriver;
        }
        private DiscreteFloatGeneticDriver DiscreteFloatDriver(int possibleStateCount)
        {
            var floatDriver = ScriptableObject.CreateInstance<DiscreteFloatGeneticDriver>();
            floatDriver.DriverName = currentGeneIndex.ToString();
            floatDriver.myId = currentGeneIndex;
            floatDriver.possibleStates = Enumerable.Range(0, possibleStateCount)
                .Select(x => $"state {x}")
                .ToArray();
            currentGeneIndex++;
            return floatDriver;
        }
        private ContinuousFloatGeneticDriver FloatDriver(float min, float max)
        {
            var floatDriver = ScriptableObject.CreateInstance<ContinuousFloatGeneticDriver>();
            floatDriver.DriverName = currentGeneIndex.ToString();
            floatDriver.myId = currentGeneIndex;
            floatDriver.minValue = min;
            floatDriver.maxValue = max;
            currentGeneIndex++;
            return floatDriver;
        }
    }
}
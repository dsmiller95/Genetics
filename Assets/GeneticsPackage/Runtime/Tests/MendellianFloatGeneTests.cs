using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Genetics.Genes;
using Genetics.GeneticDrivers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Genetics
{
    public class MendellianFloatGeneTests
    {
        [Test]
        public void MendelianFloatGeneWeightsDistributionBasedOnDominanceInCenter()
        {
            currentGeneIndex = 0;

            var floatGene = ScriptableObject.CreateInstance<MendelianFloatGene>();
            var floatDriver = FloatDriver();
            floatGene.floatOutput = floatDriver;
            floatGene.rangeMin = 0f;
            floatGene.rangeMax = 10f;
            floatGene.relativeDominantRange = 0.5f;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { floatGene };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { floatDriver };
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 1.001f;

            var totalSamples = 10000;
            var samplingResult = genomeSampler.AnalyzeGenome(totalSamples, new System.Random(2));

            Assert.AreEqual(1, samplingResult.floatResults.Length);
            var floatHistogram = samplingResult.floatResults[0];
            Assert.AreEqual(floatDriver, floatHistogram.analyzedDriver);
            Assert.AreEqual(1.001f, floatHistogram.bucketSeperation);


            AssertEquivalentDistribution(
                // each value is a probability estimate of each outcome. should peak in the center.
                new double[] {
                    .1 * .1,
                    2 * .1 * .1 + .1 * .1,
                    2 * .1 * .2 + .1 * .1,
                    2 * .1 * .3 + .1 * .1,
                    2 * .1 * .4 + .1 * .1,
                    2 * .1 * .4 + .1 * .1,
                    2 * .1 * .3 + .1 * .1,
                    2 * .1 * .2 + .1 * .1,
                    2 * .1 * .1 + .1 * .1,
                    .1 * .1,
                },
                floatHistogram.buckets,
                totalSamples,
                150);
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

        private DiscreteFloatGeneticDriver DiscreteFloatDriver()
        {
            var floatDriver = ScriptableObject.CreateInstance<DiscreteFloatGeneticDriver>();
            floatDriver.DriverName = currentGeneIndex.ToString();
            floatDriver.myId = currentGeneIndex;
            currentGeneIndex++;
            return floatDriver;
        }
        private FloatGeneticDriver FloatDriver()
        {
            var floatDriver = ScriptableObject.CreateInstance<ContinuousFloatGeneticDriver>();
            floatDriver.DriverName = currentGeneIndex.ToString();
            floatDriver.myId = currentGeneIndex;
            currentGeneIndex++;
            return floatDriver;
        }

        private void AssertEquivalentDistribution(double[] expectedDistribution, int[] realBuckets, int totalSamples, float allowedDeviation = -1)
        {
            Assert.AreEqual(expectedDistribution.Length, realBuckets.Length);

            if(allowedDeviation < 0)
            {
                allowedDeviation = Mathf.Sqrt(totalSamples);
            }

            var expectedToSampleNumConversion = totalSamples / expectedDistribution.Sum();
            for (int sampleTest = 0; sampleTest < expectedDistribution.Length; sampleTest++)
            {
                var expected = expectedToSampleNumConversion * expectedDistribution[sampleTest];
                Assert.AreEqual(expected, realBuckets[sampleTest], allowedDeviation, $"at bucket {sampleTest}");
            }
        }
    }
}
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
    public class GenomeAnalysisTests
    {
        [Test]
        public void GenomeSamplerSamplesSingleBooleanGene()
        {
            var boolGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            var boolDriver = ScriptableObject.CreateInstance<BooleanGeneticDriver>();
            boolDriver.DriverName = "bool1";
            boolDriver.myId = 0;
            boolGene.switchOutput = boolDriver;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { boolGene };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { boolDriver };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] {};
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 0.5f;

            var samplingResult = genomeSampler.AnalyzeGenome(1, new System.Random(0));

            Assert.AreEqual(0, samplingResult.floatResults.Length);
            Assert.AreEqual(1, samplingResult.boolResults.Length);
            var boolHistogram = samplingResult.boolResults[0];
            Assert.AreEqual(boolDriver, boolHistogram.analyzedDriver);
            Assert.AreEqual(0, boolHistogram.trueResultCount);
            Assert.AreEqual(1, boolHistogram.falseResultCount);
        }

        [Test]
        public void MendelianBooleanTendsTowardsMendelsRatio()
        {
            var boolGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            var boolDriver = ScriptableObject.CreateInstance<BooleanGeneticDriver>();
            boolDriver.DriverName = "bool1";
            boolDriver.myId = 0;
            boolGene.switchOutput = boolDriver;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { boolGene };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { boolDriver };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { };
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 0.5f;

            var samplingResult = genomeSampler.AnalyzeGenome(1000, new System.Random(0));

            Assert.AreEqual(0, samplingResult.floatResults.Length);
            Assert.AreEqual(1, samplingResult.boolResults.Length);
            var boolHistogram = samplingResult.boolResults[0];
            Assert.AreEqual(boolDriver, boolHistogram.analyzedDriver);
            Assert.AreEqual(750, boolHistogram.trueResultCount, 20);
            Assert.AreEqual(250, boolHistogram.falseResultCount, 20);
        }

        [Test]
        public void IsolatedBooleanTendsTowardsEvenSplit()
        {
            currentGeneIndex = 0;
            var boolGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            var boolDriver = BoolDriver();
            boolGene.switchOutput = boolDriver;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 1;
            chromosome.genes = new GeneEditor[] { boolGene };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { boolDriver };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { };
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 0.5f;

            var samplingResult = genomeSampler.AnalyzeGenome(10000, new System.Random(0));

            Assert.AreEqual(0, samplingResult.floatResults.Length);
            Assert.AreEqual(1, samplingResult.boolResults.Length);
            var boolHistogram = samplingResult.boolResults[0];
            Assert.AreEqual(boolDriver, boolHistogram.analyzedDriver);
            Assert.AreEqual(5000, boolHistogram.trueResultCount, 100);
            Assert.AreEqual(5000, boolHistogram.falseResultCount, 100);
        }
        [Test]
        public void CompositeAggregatorTendsTowardsBinomial()
        {
            currentGeneIndex = 0;

            var boolGenes = ScriptableObject.CreateInstance<MultiBooleanGene>();
            var boolDrivers = Enumerable.Repeat(0, 3).Select(x => BoolDriver()).ToArray();
            boolGenes.outputDrivers = boolDrivers;
            boolGenes.dominantValues = boolDrivers.Select(x => false).ToArray();
            
            var floatOutput = FloatDriver();
            var boolInterpretor = ScriptableObject.CreateInstance<MultiBooleanGeneAccumulationSelector>();
            boolInterpretor.booleanInputs = boolDrivers;
            boolInterpretor.floatOutput = floatOutput;


            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 1;
            chromosome.genes = new GeneEditor[] { boolGenes, boolInterpretor };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { floatOutput};
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 1f;

            var totalSamples = 10000;
            var samplingResult = genomeSampler.AnalyzeGenome(totalSamples, new System.Random(0));

            Assert.AreEqual(1, samplingResult.floatResults.Length);
            Assert.AreEqual(0, samplingResult.boolResults.Length);
            var floatHistogram = samplingResult.floatResults[0];
            Assert.AreEqual(floatOutput, floatHistogram.analyzedDriver);
            Assert.AreEqual(1f, floatHistogram.bucketSeperation);

            AssertEquivalentDistribution(
                new double[] { 1, 3, 3, 1 },
                floatHistogram.buckets,
                totalSamples);
        }
        [Test]
        public void CompositeMendelianAggregatorTendsTowardsWeightedBinomial()
        {
            currentGeneIndex = 0;

            var boolGenes = ScriptableObject.CreateInstance<MultiBooleanGene>();
            var boolDrivers = Enumerable.Repeat(0, 2).Select(x => BoolDriver()).ToArray();
            boolGenes.outputDrivers = boolDrivers;
            boolGenes.dominantValues = boolDrivers.Select(x => false).ToArray();

            var floatOutput = FloatDriver();
            var boolInterpretor = ScriptableObject.CreateInstance<MultiBooleanGeneAccumulationSelector>();
            boolInterpretor.booleanInputs = boolDrivers;
            boolInterpretor.floatOutput = floatOutput;


            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { boolGenes, boolInterpretor };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { floatOutput };
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 1f;

            var totalSamples = 10000;
            var samplingResult = genomeSampler.AnalyzeGenome(totalSamples, new System.Random(2));

            Assert.AreEqual(1, samplingResult.floatResults.Length);
            Assert.AreEqual(0, samplingResult.boolResults.Length);
            var floatHistogram = samplingResult.floatResults[0];
            Assert.AreEqual(floatOutput, floatHistogram.analyzedDriver);
            Assert.AreEqual(1f, floatHistogram.bucketSeperation);

            AssertEquivalentDistribution(
                // each binomial value is the proper probability of each outcome
                new double[] { 1 * .75f * .75f, 2 * .75f * .25f, 1 * .25f * .25f},
                floatHistogram.buckets,
                totalSamples);
        }
        [Test]
        public void CompositeMendelianAggregatorTendsTowardsCenterWeightedBinomialWhenEvenlyDistributedDominance()
        {
            currentGeneIndex = 0;

            var boolGenes = ScriptableObject.CreateInstance<MultiBooleanGene>();
            var boolDrivers = Enumerable.Repeat(0, 2).Select(x => BoolDriver()).ToArray();
            boolGenes.outputDrivers = boolDrivers;
            boolGenes.dominantValues = boolDrivers.Select((x, i) => (i % 2) == 0).ToArray();

            var floatOutput = FloatDriver();
            var boolInterpretor = ScriptableObject.CreateInstance<MultiBooleanGeneAccumulationSelector>();
            boolInterpretor.booleanInputs = boolDrivers;
            boolInterpretor.floatOutput = floatOutput;


            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { boolGenes, boolInterpretor };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { floatOutput };
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 1f;

            var totalSamples = 10000;
            var samplingResult = genomeSampler.AnalyzeGenome(totalSamples, new System.Random(2));

            Assert.AreEqual(1, samplingResult.floatResults.Length);
            Assert.AreEqual(0, samplingResult.boolResults.Length);
            var floatHistogram = samplingResult.floatResults[0];
            Assert.AreEqual(floatOutput, floatHistogram.analyzedDriver);
            Assert.AreEqual(1f, floatHistogram.bucketSeperation);

            AssertEquivalentDistribution(
                // each binomial value is the proper probability of each outcome
                new double[] {
                    .75 * .25,
                    .25 * .25 + .75 * .75,
                    .25 * .75 },
                floatHistogram.buckets,
                totalSamples);
        }
        [Test]
        public void CompositeMendelianAggregatorTendsTowardsUnweightedBinomialWhenEvenlyDistributedDominance()
        {
            currentGeneIndex = 0;

            var boolGenes = ScriptableObject.CreateInstance<MultiBooleanGene>();
            var boolDrivers = Enumerable.Repeat(0, 4).Select(x => BoolDriver()).ToArray();
            boolGenes.outputDrivers = boolDrivers;
            boolGenes.dominantValues = boolDrivers.Select((x, i) => (i % 2) == 0).ToArray();

            var floatOutput = FloatDriver();
            var boolInterpretor = ScriptableObject.CreateInstance<MultiBooleanGeneAccumulationSelector>();
            boolInterpretor.booleanInputs = boolDrivers;
            boolInterpretor.floatOutput = floatOutput;


            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { boolGenes, boolInterpretor };

            var genome = ScriptableObject.CreateInstance<GenomeEditor>();
            genome.chromosomes = new ChromosomeEditor[] { chromosome };

            var genomeSampler = ScriptableObject.CreateInstance<GenomeDistributionSampler>();
            genomeSampler.boolsOfInterest = new GeneticDriver<bool>[] { };
            genomeSampler.floatsOfInterest = new GeneticDriver<float>[] { floatOutput };
            genomeSampler.targetGenome = genome;
            genomeSampler.defaultHistogramBucketSize = 1f;

            var totalSamples = 10000;
            var samplingResult = genomeSampler.AnalyzeGenome(totalSamples, new System.Random(5));

            Assert.AreEqual(1, samplingResult.floatResults.Length);
            Assert.AreEqual(0, samplingResult.boolResults.Length);
            var floatHistogram = samplingResult.floatResults[0];
            Assert.AreEqual(floatOutput, floatHistogram.analyzedDriver);
            Assert.AreEqual(1f, floatHistogram.bucketSeperation);

            var pureProbability = .75 * .75 * .25 * .25;
            var oneDifferentProbability =
                2 * .25 * .75 * .25 * .25 +
                2 * .75 * .75 * .75 * .25;
            var twoDifferentProbability =
                .25 * .25 * .25 * .25 +
                4 * .25 * .75 * .75 * .25 +
                .75 * .75 * .75 * .75;

            AssertEquivalentDistribution(
                // each binomial value is the proper probability of each outcome
                new double[] {
                    pureProbability,
                    oneDifferentProbability,
                    twoDifferentProbability,
                    oneDifferentProbability,
                    pureProbability},
                floatHistogram.buckets,
                totalSamples);
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
        private FloatGeneticDriver FloatDriver()
        {
            var floatDriver = ScriptableObject.CreateInstance<FloatGeneticDriver>();
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
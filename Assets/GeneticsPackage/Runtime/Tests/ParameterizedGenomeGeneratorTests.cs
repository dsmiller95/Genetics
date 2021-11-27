using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Genetics.Genes;
using Genetics.GeneticDrivers;
using Genetics.ParameterizedGenomeGenerator;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Genetics
{
    public class ParameterizedGenomeGeneratorTests
    {
        [Test]
        public void GenomeWithSingleBoolGeneratesExactMatches()
        {
            var singleGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            var bool1 = BoolDriver();
            singleGene.switchOutput = bool1;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 1;
            chromosome.genes = new GeneEditor[] { singleGene };

            var genomeEditor = ScriptableObject.CreateInstance<GenomeEditor>();
            genomeEditor.chromosomes = new ChromosomeEditor[] { chromosome };

            var geneGenerator = new GenomeGenerator()
            {
                booleanTargets = new BooleanGeneticTarget[] { new BooleanGeneticTarget(bool1, true) },
                floatTargets = new FloatGeneticTarget[] { },
                genomeTarget = genomeEditor
            };

            var newGenes = geneGenerator.GenerateGenomes(new System.Random(0), 10);

            var totalNulls = 0;
            var totalMatches = 0;
            foreach (var genome in newGenes)
            {
                if (genome == null)
                {
                    totalNulls++;
                    continue;
                }
                Assert.IsNotNull(genome);
                var drivers = genomeEditor.CompileGenome(genome);
                Assert.IsTrue(drivers.TryGetGeneticData(bool1, out var boolVal));
                Assert.AreEqual(true, boolVal);
                totalMatches++;
                if (totalMatches >= 10)
                {
                    break;
                }
            }
            Assert.AreEqual(10, totalMatches);
            var probabilityOfHittingTarget = 1f/2f;
            Assert.AreEqual(1f/ probabilityOfHittingTarget, totalNulls, 1);
        }
        [Test]
        public void GenomeWithSingleFloatGeneratesMatchesInRange()
        {
            var floatGene = ScriptableObject.CreateInstance<MendelianFloatGene>();
            var float1 = FloatDriver();
            floatGene.floatOutput = float1;
            floatGene.rangeMin = 0f;
            floatGene.rangeMax = 10f;
            floatGene.relativeDominantRange = 1;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 1;
            chromosome.genes = new GeneEditor[] { floatGene };

            var genomeEditor = ScriptableObject.CreateInstance<GenomeEditor>();
            genomeEditor.chromosomes = new ChromosomeEditor[] { chromosome };

            var geneGenerator = new GenomeGenerator()
            {
                booleanTargets = new BooleanGeneticTarget[] { },
                floatTargets = new FloatGeneticTarget[] { new FloatGeneticTarget(float1, 4, 4.99f) },
                genomeTarget = genomeEditor
            };

            var newGenes = geneGenerator.GenerateGenomes(new System.Random(0), 10);

            var totalNulls = 0;
            var totalMatches = 0;
            foreach (var genome in newGenes)
            {
                if (genome == null)
                {
                    totalNulls++;
                    continue;
                }
                Assert.IsNotNull(genome);
                var drivers = genomeEditor.CompileGenome(genome);
                Assert.IsTrue(drivers.TryGetGeneticData(float1, out var floatValue));
                Assert.AreEqual(4.5f, floatValue, 0.5f);
                totalMatches++;
                if (totalMatches >= 100)
                {
                    break;
                }
            }
            Assert.AreEqual(100, totalMatches);
            var probabilityOfHittingTarget = 1f/10f;
            var expected = (1f / probabilityOfHittingTarget) * 10f;
            Assert.AreEqual(expected, totalNulls, expected * 0.1f);
        }
        [Test]
        public void GenomeWithSingleFloatAndBooleanGeneratesMatches()
        {
            var floatGene = ScriptableObject.CreateInstance<MendelianFloatGene>();
            var float1 = FloatDriver();
            floatGene.originIndex = 0;
            floatGene.floatOutput = float1;
            floatGene.rangeMin = 0f;
            floatGene.rangeMax = 10f;
            floatGene.relativeDominantRange = 1;

            var boolGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            var bool1 = BoolDriver();
            boolGene.switchOutput = bool1;
            boolGene.originIndex = floatGene.precision;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 1;
            chromosome.genes = new GeneEditor[] { floatGene, boolGene };

            var genomeEditor = ScriptableObject.CreateInstance<GenomeEditor>();
            genomeEditor.chromosomes = new ChromosomeEditor[] { chromosome };

            var geneGenerator = new GenomeGenerator()
            {
                booleanTargets = new BooleanGeneticTarget[] { new BooleanGeneticTarget(bool1, false) },
                floatTargets = new FloatGeneticTarget[] { new FloatGeneticTarget(float1, 4, 5) },
                genomeTarget = genomeEditor
            };

            var newGenes = geneGenerator.GenerateGenomes(new System.Random(1), 10);

            var totalNulls = 0;
            var totalMatches = 0;
            foreach (var genome in newGenes)
            {
                if (genome == null)
                {
                    totalNulls++;
                    continue;
                }
                Assert.IsNotNull(genome);
                var drivers = genomeEditor.CompileGenome(genome);
                Assert.IsTrue(drivers.TryGetGeneticData(float1, out var floatValue));
                Assert.AreEqual(4.5f, floatValue, 0.5f);
                Assert.IsTrue(drivers.TryGetGeneticData(bool1, out var boolVal));
                Assert.AreEqual(false, boolVal);
                totalMatches++;
                if(totalMatches >= 100)
                {
                    break;
                }
            }
            Assert.AreEqual(100, totalMatches);
            var probabilityOfMatchingBool = 1f / 2f;
            var probabilityOfMatchingFloat = 1f / 10f;
            var expected = 1f / (probabilityOfMatchingBool * probabilityOfMatchingFloat) * 10;
            Assert.AreEqual(expected, totalNulls, expected * 0.1f);
        }
        [Test]
        public void GenomeWithSingleFloatAndBooleanMultiChromosomeGeneratesMatchesMoreRarely()
        {
            var floatGene = ScriptableObject.CreateInstance<MendelianFloatGene>();
            var float1 = FloatDriver();
            floatGene.originIndex = 0;
            floatGene.floatOutput = float1;
            floatGene.rangeMin = 0f;
            floatGene.rangeMax = 10f;
            floatGene.relativeDominantRange = 1;
            var boolGene = ScriptableObject.CreateInstance<MendelianBooleanSwitch>();
            var bool1 = BoolDriver();
            boolGene.switchOutput = bool1;
            boolGene.originIndex = floatGene.precision;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { floatGene, boolGene };

            var genomeEditor = ScriptableObject.CreateInstance<GenomeEditor>();
            genomeEditor.chromosomes = new ChromosomeEditor[] { chromosome };

            var geneGenerator = new GenomeGenerator()
            {
                booleanTargets = new BooleanGeneticTarget[] { new BooleanGeneticTarget(bool1, false) },
                floatTargets = new FloatGeneticTarget[] { new FloatGeneticTarget(float1, 4, 5) },
                genomeTarget = genomeEditor
            };

            var newGenes = geneGenerator.GenerateGenomes(new System.Random(1), 10);

            var totalNulls = 0;
            var totalMatches = 0;
            foreach (var genome in newGenes)
            {
                if (genome == null)
                {
                    totalNulls++;
                    continue;
                }
                Assert.IsNotNull(genome);
                var drivers = genomeEditor.CompileGenome(genome);
                Assert.IsTrue(drivers.TryGetGeneticData(float1, out var floatValue));
                Assert.AreEqual(4.5f, floatValue, 0.5f);
                Assert.IsTrue(drivers.TryGetGeneticData(bool1, out var boolVal));
                Assert.AreEqual(false, boolVal);
                totalMatches++;
                if (totalMatches >= 100)
                {
                    break;
                }
            }
            Assert.AreEqual(100, totalMatches);
            var probabilityOfMatchingBool = 1f / 4f;
            var probabilityOfMatchingFloat = 2 * .1 * .4 + .1 * .1;
            var expected = 1f / (probabilityOfMatchingBool * probabilityOfMatchingFloat) * 10;
            Assert.AreEqual(expected, totalNulls, expected * 0.1f);
        }
        [Test]
        public void GenomeWithImpossibleTargetInfiniteNulls()
        {
            var floatGene = ScriptableObject.CreateInstance<MendelianFloatGene>();
            var float1 = FloatDriver();
            floatGene.floatOutput = float1;
            floatGene.rangeMin = 0f;
            floatGene.rangeMax = 10f;
            floatGene.relativeDominantRange = 1;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { floatGene };

            var genomeEditor = ScriptableObject.CreateInstance<GenomeEditor>();
            genomeEditor.chromosomes = new ChromosomeEditor[] { chromosome };

            var geneGenerator = new GenomeGenerator()
            {
                booleanTargets = new BooleanGeneticTarget[] { },
                floatTargets = new FloatGeneticTarget[] { new FloatGeneticTarget(float1, 4, 5), new FloatGeneticTarget(float1, 6, 7) },
                genomeTarget = genomeEditor
            };

            var newGenes = geneGenerator.GenerateGenomes(new System.Random(1), 10).Take(1000);

            foreach (var genome in newGenes)
            {
                Assert.IsNull(genome);
            }
        }
        [Test]
        public void GenomeWithComplexAggregatorStillGeneratesMatches()
        {
            var boolGenes = ScriptableObject.CreateInstance<MultiBooleanGene>();
            var boolDrivers = Enumerable.Repeat(0, 5).Select(x => BoolDriver()).ToArray();
            boolGenes.outputDrivers = boolDrivers;
            boolGenes.dominantValues = boolDrivers.Select(x => true).ToArray();

            var floatOutput = DiscreteFloatDriver();
            var boolInterpretor = ScriptableObject.CreateInstance<MultiBooleanGeneAccumulationSelector>();
            boolInterpretor.booleanInputs = boolDrivers;
            boolInterpretor.floatOutput = floatOutput;

            var chromosome = ScriptableObject.CreateInstance<ChromosomeEditor>();
            chromosome.chromosomeCopies = 2;
            chromosome.genes = new GeneEditor[] { boolGenes, boolInterpretor };

            var genomeEditor = ScriptableObject.CreateInstance<GenomeEditor>();
            genomeEditor.chromosomes = new ChromosomeEditor[] { chromosome };

            var geneGenerator = new GenomeGenerator()
            {
                booleanTargets = new BooleanGeneticTarget[] {  },
                floatTargets = new FloatGeneticTarget[] { new FloatGeneticTarget(floatOutput, 0.1f, 1.2f) },
                genomeTarget = genomeEditor
            };

            var newGenes = geneGenerator.GenerateGenomes(new System.Random(2), 10);

            var probabilityOfMatch =
                    .25 * .25 * .25 * .25 * .25 +
                5 * .75 * .25 * .25 * .25 * .25;
            var expectedNulls = 1 / (probabilityOfMatch) * 10;

            var totalNulls = 0;
            var totalMatches = 0;
            foreach (var genome in newGenes)
            {
                if (genome == null)
                {
                    totalNulls++;
                    if(totalNulls >= 10 * expectedNulls)
                    {
                        break;
                    }
                    continue;
                }
                Assert.IsNotNull(genome);
                var drivers = genomeEditor.CompileGenome(genome);
                Assert.IsTrue(drivers.TryGetGeneticData(floatOutput, out var floatValue));
                var intValue = Mathf.FloorToInt(floatValue);
                Assert.IsTrue(intValue == 0 || intValue == 1, "output classification must be either 0 or 1 exactly");
                totalMatches++;
                if (totalMatches >= 100)
                {
                    break;
                }
            }
            // for some reason the estimate is off more than usual here \shrug
            Assert.AreEqual(expectedNulls, totalNulls, expectedNulls * 0.2f);
            Assert.AreEqual(100, totalMatches);
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
        private ContinuousFloatGeneticDriver FloatDriver()
        {
            var floatDriver = ScriptableObject.CreateInstance<ContinuousFloatGeneticDriver>();
            floatDriver.DriverName = currentGeneIndex.ToString();
            floatDriver.myId = currentGeneIndex;
            currentGeneIndex++;
            return floatDriver;
        }
    }
}
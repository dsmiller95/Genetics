using Genetics.Genes;
using Genetics.GeneticDrivers;
using Genetics.ParameterizedGenomeGenerator;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics
{
    public class GeneticTargetCompositionTests
    {
        private static readonly float delta = 0.1f;

        private enum BoundaryClassification
        {
            /// <summary>
            /// a sample at the boundary matches,
            /// a sample below the boundary does not match,
            /// and a sample above the boundary does match
            /// </summary>
            MIN_BOUND,
            /// <summary>
            /// a sample at the boundary matches,
            /// a sample below the boundary does match,
            /// and a sample above the boundary does not match
            /// </summary>
            MAX_BOUND,
            /// <summary>
            /// the boundary and all samples around it match
            /// </summary>
            ALL_MATCH,
            /// <summary>
            /// the boundary and all samples around it do not match
            /// </summary>
            NO_MATCH,
            INVALID
        }

        private BoundaryClassification ClassifyBoundary(float boundary, GenomeTargetContainer target, FloatGeneticDriver driver, CompiledGeneticDrivers contextDrivers)
        {
            contextDrivers.SetGeneticDriverData(driver, boundary - delta, true);
            var downMatch = target.DriversMatch(contextDrivers);

            contextDrivers.SetGeneticDriverData(driver, boundary, true);
            var boundMatch = target.DriversMatch(contextDrivers);

            contextDrivers.SetGeneticDriverData(driver, boundary + delta, true);
            var upMatch = target.DriversMatch(contextDrivers);

            if (!downMatch && !boundMatch && !upMatch)
            {
                return BoundaryClassification.NO_MATCH;
            }
            if (downMatch && boundMatch && upMatch)
            {
                return BoundaryClassification.ALL_MATCH;
            }
            if (downMatch && boundMatch && !upMatch)
            {
                return BoundaryClassification.MAX_BOUND;
            }
            if (!downMatch && boundMatch && upMatch)
            {
                return BoundaryClassification.MIN_BOUND;
            }
            return BoundaryClassification.INVALID;
        }

        private void MinBoundAssert(float boundary, GenomeTargetContainer target, FloatGeneticDriver driver, CompiledGeneticDrivers contextDrivers)
        {
            Assert.AreEqual(BoundaryClassification.MIN_BOUND, ClassifyBoundary(boundary, target, driver, contextDrivers));
        }
        private void MaxBoundAssert(float boundary, GenomeTargetContainer target, FloatGeneticDriver driver, CompiledGeneticDrivers contextDrivers)
        {
            Assert.AreEqual(BoundaryClassification.MAX_BOUND, ClassifyBoundary(boundary, target, driver, contextDrivers));
        }

        private void AllMatchAssert(float boundary, GenomeTargetContainer target, FloatGeneticDriver driver, CompiledGeneticDrivers contextDrivers)
        {
            Assert.AreEqual(BoundaryClassification.ALL_MATCH, ClassifyBoundary(boundary, target, driver, contextDrivers));
        }
        private void NoMatchAssert(float boundary, GenomeTargetContainer target, FloatGeneticDriver driver, CompiledGeneticDrivers contextDrivers)
        {
            Assert.AreEqual(BoundaryClassification.NO_MATCH, ClassifyBoundary(boundary, target, driver, contextDrivers));
        }

        private void AssertRangeCount(int rangeCount, BrokenFloatRange testTarget)
        {
            Assert.AreEqual(rangeCount, testTarget.GetRepresentativeRange().Count());
        }

        private FloatGeneticDriver GetFloatDriver(string name)
        {
            var driver = ScriptableObject.CreateInstance<ContinuousFloatGeneticDriver>();
            driver.DriverName = name;
            return driver;
        }

        private GenomeTargetContainer CreateGenomeTarget(params FloatGeneticDriver[] drivers)
        {
            var target = ScriptableObject.CreateInstance<GenomeTargetContainer>();
            var genome = ScriptableObject.CreateInstance<GenomeEditor>();

            genome.geneInterpretors = drivers.Select(x =>
            {
                var newGene = ScriptableObject.CreateInstance<MendelianFloatGene>();
                newGene.floatOutput = x;
                return newGene;
            }).ToArray();
            genome.chromosomes = new ChromosomeEditor[0];

            target.targetGenome = genome;
            return target;
        }

        [Test]
        public void MatchesBasedOnMultipleRangesWhenSetExclusive()
        {
            var driver1 = GetFloatDriver("driver1");
            var driver2 = GetFloatDriver("driver2");
            var target = CreateGenomeTarget(driver1, driver2);

            target.SetExclusiveTarget(new FloatGeneticTarget(driver1, 2, 5));
            target.SetExclusiveTarget(new FloatGeneticTarget(driver2, 6, 8));

            var drivers = new CompiledGeneticDrivers();
            drivers.SetGeneticDriverData(driver1, 3, true);
            drivers.SetGeneticDriverData(driver2, 7, true);

            MinBoundAssert(2, target, driver1, drivers);
            MaxBoundAssert(5, target, driver1, drivers);
            drivers.SetGeneticDriverData(driver1, 3, true);

            MinBoundAssert(6, target, driver2, drivers);
            MaxBoundAssert(8, target, driver2, drivers);
        }

        [Test]
        public void MatchesBasedOnMultipleRangesWhenSetViaInspector()
        {
            var driver1 = GetFloatDriver("driver1");
            var driver2 = GetFloatDriver("driver2");
            var target = CreateGenomeTarget(driver1, driver2);

            target.floatTargets = new List<FloatGeneticTarget>
            {
                new FloatGeneticTarget(driver1, 2, 5),
                new FloatGeneticTarget(driver2, 6, 8)
            };

            var drivers = new CompiledGeneticDrivers();
            drivers.SetGeneticDriverData(driver1, 3, true);
            drivers.SetGeneticDriverData(driver2, 7, true);

            MinBoundAssert(2, target, driver1, drivers);
            MaxBoundAssert(5, target, driver1, drivers);
            drivers.SetGeneticDriverData(driver1, 3, true);

            MinBoundAssert(6, target, driver2, drivers);
            MaxBoundAssert(8, target, driver2, drivers);
        }

        [Test]
        public void MatchesWhenOverridingWithSetExclusive()
        {
            var driver1 = GetFloatDriver("driver1");
            var driver2 = GetFloatDriver("driver2");
            var target = CreateGenomeTarget(driver1, driver2);

            target.floatTargets = new List<FloatGeneticTarget>
            {
                new FloatGeneticTarget(driver1, 10, 15)
            };

            target.SetExclusiveTarget(new FloatGeneticTarget(driver1, 2, 5));
            target.SetExclusiveTarget(new FloatGeneticTarget(driver2, 6, 8));

            var drivers = new CompiledGeneticDrivers();
            drivers.SetGeneticDriverData(driver1, 3, true);
            drivers.SetGeneticDriverData(driver2, 7, true);

            MinBoundAssert(2, target, driver1, drivers);
            MaxBoundAssert(5, target, driver1, drivers);
            drivers.SetGeneticDriverData(driver1, 3, true);

            MinBoundAssert(6, target, driver2, drivers);
            MaxBoundAssert(8, target, driver2, drivers);
        }

        [Test]
        public void MatchesWhenIncludingAdditionalRange()
        {
            var driver1 = GetFloatDriver("driver1");
            var driver2 = GetFloatDriver("driver2");
            var target = CreateGenomeTarget(driver1, driver2);

            target.floatTargets = new List<FloatGeneticTarget>
            {
                new FloatGeneticTarget(driver1, 2, 3)
            };

            target.IncludeTarget(new FloatGeneticTarget(driver1, 3, 5));
            target.IncludeTarget(new FloatGeneticTarget(driver2, 6, 8));

            var drivers = new CompiledGeneticDrivers();
            drivers.SetGeneticDriverData(driver1, 3, true);
            drivers.SetGeneticDriverData(driver2, 7, true);

            MinBoundAssert(2, target, driver1, drivers);
            AllMatchAssert(3, target, driver1, drivers);
            MaxBoundAssert(5, target, driver1, drivers);
            drivers.SetGeneticDriverData(driver1, 3, true);

            AllMatchAssert(6, target, driver2, drivers);
            AllMatchAssert(8, target, driver2, drivers);
        }

        [Test]
        public void MatchesWhenExcludingRange()
        {
            var driver1 = GetFloatDriver("driver1");
            var driver2 = GetFloatDriver("driver2");
            var target = CreateGenomeTarget(driver1, driver2);

            target.floatTargets = new List<FloatGeneticTarget>
            {
                new FloatGeneticTarget(driver1, 2, 7)
            };

            target.ExcludeTarget(new FloatGeneticTarget(driver1, 5, 8));
            target.ExcludeTarget(new FloatGeneticTarget(driver2, 6, 8));

            var drivers = new CompiledGeneticDrivers();
            drivers.SetGeneticDriverData(driver1, 3, true);
            drivers.SetGeneticDriverData(driver2, 0, true);

            MinBoundAssert(2, target, driver1, drivers);
            MaxBoundAssert(5, target, driver1, drivers);
            NoMatchAssert(8, target, driver1, drivers);
            drivers.SetGeneticDriverData(driver1, 3, true);

            MaxBoundAssert(6, target, driver2, drivers);
            NoMatchAssert(7, target, driver2, drivers);
            MinBoundAssert(8, target, driver2, drivers);
        }


        [Test]
        public void MatchesWhenMergingTargetSets()
        {
            var driver1 = GetFloatDriver("driver1");
            var driver2 = GetFloatDriver("driver2");
            var target = CreateGenomeTarget(driver1, driver2);

            target.floatTargets = new List<FloatGeneticTarget>
            {
                new FloatGeneticTarget(driver1, 2, 3)
            };

            var mergeTarget = CreateGenomeTarget(driver1, driver2);
            mergeTarget.floatTargets = new List<FloatGeneticTarget>
            {
                new FloatGeneticTarget(driver1, 2, 5),
                new FloatGeneticTarget(driver2, 6, 8),
            };
            target.MergeOtherIn(mergeTarget);

            var drivers = new CompiledGeneticDrivers();
            drivers.SetGeneticDriverData(driver1, 3, true);
            drivers.SetGeneticDriverData(driver2, 0, true);

            MinBoundAssert(2, target, driver1, drivers);
            AllMatchAssert(3, target, driver1, drivers);
            MaxBoundAssert(5, target, driver1, drivers);
            drivers.SetGeneticDriverData(driver1, 3, true);

            AllMatchAssert(6, target, driver2, drivers);
            AllMatchAssert(7, target, driver2, drivers);
            AllMatchAssert(8, target, driver2, drivers);
        }
    }
}
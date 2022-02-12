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
    public class BrokenFloatRangeComparisonTests
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

        private BoundaryClassification ClassifyBoundary(float boundary, BrokenFloatRange testTarget)
        {
            var downMatch = testTarget.Matches(boundary - delta);
            var boundMatch = testTarget.Matches(boundary);
            var upMatch = testTarget.Matches(boundary + delta);

            if(!downMatch && !boundMatch && !upMatch)
            {
                return BoundaryClassification.NO_MATCH;
            }
            if(downMatch && boundMatch && upMatch)
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

        private void MinBoundAssert(float boundary, BrokenFloatRange testTarget)
        {
            Assert.AreEqual(BoundaryClassification.MIN_BOUND, ClassifyBoundary(boundary, testTarget));
        }
        private void MaxBoundAssert(float boundary, BrokenFloatRange testTarget)
        {
            Assert.AreEqual(BoundaryClassification.MAX_BOUND, ClassifyBoundary(boundary, testTarget));
        }

        private void AllMatchAssert(float boundary, BrokenFloatRange testTarget)
        {
            Assert.AreEqual(BoundaryClassification.ALL_MATCH, ClassifyBoundary(boundary, testTarget));
        }
        private void NoMatchAssert(float boundary, BrokenFloatRange testTarget)
        {
            Assert.AreEqual(BoundaryClassification.NO_MATCH, ClassifyBoundary(boundary, testTarget));
        }

        private void AssertRangeCount(int rangeCount, BrokenFloatRange testTarget)
        {
            Assert.AreEqual(rangeCount, testTarget.GetRepresentativeRange().Count());
        }

        [Test]
        public void SamplesBoundariesOfSimpleContinuousRange()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: false);
            MinBoundAssert(2.3f, simpleTarget);
            MaxBoundAssert(5.7f, simpleTarget);

            AssertRangeCount(1, simpleTarget);
        }

        [Test]
        public void SamplesBoundariesOfSimpleDiscreteRange()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: true);
            MinBoundAssert(3f, simpleTarget);
            MaxBoundAssert(6f - delta, simpleTarget);
            AssertRangeCount(1, simpleTarget);
        }
        [Test]
        public void SamplesBoundariesOfPointDiscreteRange()
        {
            var simpleTarget = new BrokenFloatRange(3f, 3f, discrete: true);
            MinBoundAssert(3f, simpleTarget);
            MaxBoundAssert(4f - delta, simpleTarget);
            AssertRangeCount(1, simpleTarget);
        }

        [Test]
        public void MergesNonOverlappingRanges()
        {
            var primeRange = new BrokenFloatRange(1, 3);
            primeRange.MergeIn(new BrokenFloatRange(5, 9));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(3, primeRange);

            MinBoundAssert(5, primeRange);
            MaxBoundAssert(9, primeRange);

            AssertRangeCount(2, primeRange);
        }

        [Test]
        public void MergesTouchingRanges()
        {
            var primeRange = new BrokenFloatRange(1, 3);
            primeRange.MergeIn(new BrokenFloatRange(3, 9));

            MinBoundAssert(1, primeRange);
            AllMatchAssert(3, primeRange);
            MaxBoundAssert(9, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void MergesAdjacentPointDiscreteRanges()
        {
            var primeRange = new BrokenFloatRange(1, 1, discrete: true);
            primeRange.MergeIn(new BrokenFloatRange(2, 2, discrete: true));

            MinBoundAssert(1, primeRange);
            AllMatchAssert(2, primeRange);
            MaxBoundAssert(3 - delta, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void MergesOverlappingRanges()
        {
            var primeRange = new BrokenFloatRange(1, 3.1f);
            primeRange.MergeIn(new BrokenFloatRange(2.9f, 9));

            MinBoundAssert(1, primeRange);
            AllMatchAssert(3, primeRange);
            MaxBoundAssert(9, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void MergesOverlappingPointDiscreteRanges()
        {
            var primeRange = new BrokenFloatRange(1, 1, discrete: true);
            primeRange.MergeIn(new BrokenFloatRange(1.3f, 1.5f, discrete: true));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2 - delta, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void MergesCompletelyOverlappingRanges()
        {
            var primeRange = new BrokenFloatRange(1, 7f);
            primeRange.MergeIn(new BrokenFloatRange(2.6f, 5f));

            MinBoundAssert(1, primeRange);
            AllMatchAssert(2.6f, primeRange);
            AllMatchAssert(5f, primeRange);
            MaxBoundAssert(7, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void MergesOverlappingRangesAtMultipleSpots()
        {
            var primeRange = new BrokenFloatRange(1, 3.1f);
            primeRange.MergeIn(new BrokenFloatRange(4, 5f));
            primeRange.MergeIn(new BrokenFloatRange(8, 10f));
            primeRange.MergeIn(new BrokenFloatRange(2.9f, 9));

            MinBoundAssert(1, primeRange);
            AllMatchAssert(3, primeRange);
            AllMatchAssert(9, primeRange);
            MaxBoundAssert(10, primeRange);

            AssertRangeCount(1, primeRange);
        }

        [Test]
        public void ExcludesNonOverlappingRange()
        {
            var primeRange = new BrokenFloatRange(1, 3);
            primeRange.Exclude(new BrokenFloatRange(5, 9));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(3, primeRange);

            NoMatchAssert(5, primeRange);
            NoMatchAssert(9, primeRange);

            AssertRangeCount(1, primeRange);
        }

        [Test]
        public void ExcludesNonOverlappingDiscretePoints()
        {
            var primeRange = new BrokenFloatRange(1, 1, discrete: true);
            primeRange.Exclude(new BrokenFloatRange(3, 3, discrete: true));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2 - delta, primeRange);

            NoMatchAssert(3, primeRange);
            NoMatchAssert(4 - delta, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void ExcludesOverlappingDiscretePoints()
        {
            var primeRange = new BrokenFloatRange(1, 2, discrete: true);
            MinBoundAssert(1, primeRange);
            AllMatchAssert(2, primeRange);
            MaxBoundAssert(3 - delta, primeRange);

            primeRange.Exclude(new BrokenFloatRange(2, 2, discrete: true));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2 - delta, primeRange);
            NoMatchAssert(3 - delta, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void ExcludesOverlappingDiscretePointAndWeirdRange()
        {
            var primeRange = new BrokenFloatRange(1, 2, discrete: true);
            primeRange.Exclude(new BrokenFloatRange(1.9f, 2.5f, discrete: true));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2 - delta, primeRange);

            NoMatchAssert(3 - delta, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void ExcludesTouchingRange()
        {
            var primeRange = new BrokenFloatRange(1, 3);
            primeRange.Exclude(new BrokenFloatRange(3, 9));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(3, primeRange);

            NoMatchAssert(9, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void ExcludesOverlappingRange()
        {
            var primeRange = new BrokenFloatRange(1, 3);
            primeRange.Exclude(new BrokenFloatRange(2.3f, 9));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2.3f, primeRange);

            NoMatchAssert(3, primeRange);
            NoMatchAssert(9, primeRange);

            AssertRangeCount(1, primeRange);
        }
        [Test]
        public void ExcludesCompletelyOverlappingRange()
        {
            var primeRange = new BrokenFloatRange(1, 7);
            primeRange.Exclude(new BrokenFloatRange(2.3f, 5.2f));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2.3f, primeRange);
            NoMatchAssert(4f, primeRange);
            MinBoundAssert(5.2f, primeRange);
            MaxBoundAssert(7f, primeRange);

            AssertRangeCount(2, primeRange);
        }
        [Test]
        public void ExcludesOverlappingRangesAtMultipleSpots()
        {
            var primeRange = new BrokenFloatRange(1, 3.1f);
            primeRange.MergeIn(new BrokenFloatRange(4, 5f));
            primeRange.MergeIn(new BrokenFloatRange(8, 10f));
            primeRange.Exclude(new BrokenFloatRange(2.9f, 9));

            MinBoundAssert(1, primeRange);
            MaxBoundAssert(2.9f, primeRange);
            NoMatchAssert(4, primeRange);
            NoMatchAssert(5, primeRange);
            NoMatchAssert(8, primeRange);
            MinBoundAssert(9, primeRange);
            MaxBoundAssert(10, primeRange);


            AssertRangeCount(2, primeRange);
        }

        [Test]
        public void InvertsSimpleContinuousRange()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: false);
            simpleTarget = simpleTarget.Invert();
            MaxBoundAssert(2.3f, simpleTarget);
            MinBoundAssert(5.7f, simpleTarget);

            AssertRangeCount(2, simpleTarget);
        }

        [Test]
        public void InvertsSimpleContinuousRangeTwice()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: false);
            simpleTarget = simpleTarget.Invert();
            simpleTarget = simpleTarget.Invert();
            MinBoundAssert(2.3f, simpleTarget);
            MaxBoundAssert(5.7f, simpleTarget);

            AssertRangeCount(1, simpleTarget);
        }


        [Test]
        public void InvertsSimpleDiscreteRange()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: true);
            simpleTarget = simpleTarget.Invert();
            MaxBoundAssert(3f - delta, simpleTarget);
            MinBoundAssert(6f, simpleTarget);

            AssertRangeCount(2, simpleTarget);
        }
        [Test]
        public void InvertsSimpleDiscreteRangeTwice()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: true);
            simpleTarget = simpleTarget.Invert();
            simpleTarget = simpleTarget.Invert();
            MinBoundAssert(3f, simpleTarget);
            MaxBoundAssert(6f - delta, simpleTarget);

            AssertRangeCount(1, simpleTarget);
        }

        [Test]
        public void RepresentsDiscreteRangeSensibly()
        {
            var simpleTarget = new BrokenFloatRange(2.3f, 5.7f, discrete: true);

            var resultRange = simpleTarget.GetRepresentativeRange().ToList();
            Assert.AreEqual(1, resultRange.Count);


            var expectedRange = new FloatRange
            {
                minValue = 3,
                maxValue = 5
            };
            Assert.AreEqual(expectedRange.minValue, resultRange[0].minValue);
            Assert.AreEqual(expectedRange.maxValue, resultRange[0].maxValue);
        }
        [Test]
        public void RepresentsDiscretePointRangeSensibly()
        {
            var simpleTarget = new BrokenFloatRange(3f, 3f, discrete: true);

            var resultRange = simpleTarget.GetRepresentativeRange().ToList();
            Assert.AreEqual(1, resultRange.Count);


            var expectedRange = new FloatRange
            {
                minValue = 3,
                maxValue = 3
            };
            Assert.AreEqual(expectedRange.minValue, resultRange[0].minValue);
            Assert.AreEqual(expectedRange.maxValue, resultRange[0].maxValue);
        }

    }
}
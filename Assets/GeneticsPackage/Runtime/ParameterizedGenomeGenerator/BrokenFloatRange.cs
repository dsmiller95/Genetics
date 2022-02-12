using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.ParameterizedGenomeGenerator
{
    /// <summary>
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public struct FloatRange
    {
        public float minValue;
        public float maxValue;

        public FloatRange(float min, float max)
        {
            minValue = min;
            maxValue = max;
        }
    }

    /// <summary>
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public class BrokenFloatRange
    {
        public bool compareDiscrete;
        /// <summary>
        /// managed to ensure the boundaries stay in order
        /// </summary>
        [SerializeField]
        private List<FloatRangeBoundary> allBoundaries;
        private BrokenFloatRange(bool discrete = false)
        {
            compareDiscrete = discrete;
        }
        public BrokenFloatRange(float min, float max, bool discrete = false)
        {
            compareDiscrete = discrete;

            allBoundaries = new List<FloatRangeBoundary>();
            allBoundaries.Add(FloatRangeBoundary.MinBound(min));
            allBoundaries.Add(FloatRangeBoundary.MaxBound(max));
        }

        public BrokenFloatRange(IEnumerable<FloatRange> ranges, bool discrete = false)
        {
            compareDiscrete = discrete;

            allBoundaries = new List<FloatRangeBoundary>();

            foreach (var range in ranges)
            {
                allBoundaries.Add(FloatRangeBoundary.MinBound(range.minValue));
                allBoundaries.Add(FloatRangeBoundary.MaxBound(range.maxValue));
            }
            allBoundaries = CollapseRedundantBounds(allBoundaries, !compareDiscrete);
        }

        private struct FloatRangeBoundary
        {
            public bool isMinBound;
            public float boundary;

            public FloatRangeBoundary Invert()
            {
                return new FloatRangeBoundary
                {
                    boundary = boundary,
                    isMinBound = !isMinBound
                };
            }

            public static FloatRangeBoundary MinBound(float bound)
            {
                return new FloatRangeBoundary
                {
                    isMinBound = true,
                    boundary = bound
                };
            }
            public static FloatRangeBoundary MaxBound(float bound)
            {
                return new FloatRangeBoundary
                {
                    isMinBound = false,
                    boundary = bound
                };
            }
        }

        private class FloatRangeBoundaryComparer : IComparer<FloatRangeBoundary>
        {
            public int Compare(FloatRangeBoundary x, FloatRangeBoundary y)
            {
                if (x.boundary != y.boundary)
                {
                    return x.boundary < y.boundary ? -1 : 1;
                }
                return x.isMinBound ? -1 : 1;
            }
        }

        public IEnumerable<FloatRange> GetRepresentativeRange()
        {
            if (allBoundaries.Count <= 0)
            {
                yield break;
            }
            FloatRangeBoundary lastBoundary = allBoundaries.First();
            foreach (var boundary in allBoundaries.Skip(1))
            {
                if (lastBoundary.isMinBound)
                {
                    if (boundary.isMinBound)
                    {
                        throw new System.Exception("bad boundary format, uncollapsed double min bound");
                    }
                    yield return new FloatRange
                    {
                        minValue = lastBoundary.boundary,
                        maxValue = boundary.boundary
                    };
                }
                lastBoundary = boundary;
            }
            if (lastBoundary.isMinBound)
            {
                throw new System.Exception("bad boundary format, uncollapsed double min bound");
            }
        }

        /// <summary>
        /// merge the other target into this one, such that this target will now match based on the criteria it used to have
        ///     and the criteria of the other target
        /// </summary>
        /// <param name="otherTarget"></param>
        public void MergeIn(BrokenFloatRange otherTarget)
        {
            allBoundaries.AddRange(otherTarget.allBoundaries);
            allBoundaries = CollapseRedundantBounds(allBoundaries, !compareDiscrete);
        }

        public void Exclude(BrokenFloatRange otherTarget)
        {
            allBoundaries.AddRange(otherTarget.allBoundaries.Select(x => x.Invert()));
            allBoundaries = CollapseRedundantBounds(allBoundaries, !compareDiscrete);
        }

        public BrokenFloatRange Invert()
        {
            var nextBoundaries = new List<FloatRangeBoundary>();
            nextBoundaries.Add(FloatRangeBoundary.MinBound(float.NegativeInfinity));
            nextBoundaries.AddRange(allBoundaries.Select(x => x.Invert()));
            nextBoundaries.Add(FloatRangeBoundary.MaxBound(float.PositiveInfinity));
            nextBoundaries = CollapseRedundantBounds(nextBoundaries, !compareDiscrete);

            return new BrokenFloatRange
            {
                compareDiscrete = compareDiscrete,
                allBoundaries = nextBoundaries
            };
        }

        private static List<FloatRangeBoundary> CollapseRedundantBounds(List<FloatRangeBoundary> unorderedBounds, bool collapseZeroLength)
        {
            unorderedBounds.Sort(new FloatRangeBoundaryComparer());

            var currentDepth = 0;
            var nextBoundaries = new List<FloatRangeBoundary>();
            foreach (var boundary in unorderedBounds)
            {
                if (boundary.isMinBound)
                {
                    if (currentDepth == 0)
                    {
                        nextBoundaries.Add(boundary);
                    }
                    currentDepth++;
                }
                else
                {
                    currentDepth--;
                    if (currentDepth != 0)
                    {
                        continue;
                    }
                    if (
                        nextBoundaries[nextBoundaries.Count - 1].boundary == boundary.boundary &&
                        (collapseZeroLength || float.IsNegativeInfinity(boundary.boundary) || float.IsPositiveInfinity(boundary.boundary)))
                    {
                        // if 0-length range, omit and don't add this end bound
                        nextBoundaries.RemoveAt(nextBoundaries.Count - 1);
                        continue;
                    }
                    nextBoundaries.Add(boundary);
                }
            }

            return nextBoundaries;
        }


        public bool Matches(float value)
        {
            var intValue = Mathf.FloorToInt(value);
            foreach (var range in GetRepresentativeRange())
            {
                if (compareDiscrete && MatchesDiscrete(range, intValue))
                {
                    return true;
                }
                else if (!compareDiscrete && MatchesContinuous(range, value))
                {
                    return true;
                }
            }
            return false;
        }

        private bool MatchesContinuous(FloatRange range, float value)
        {
            return value >= range.minValue && value <= range.maxValue;
        }

        private bool MatchesDiscrete(FloatRange range, int value)
        {
            var minInt = CeilToIntSafe(range.minValue);
            var maxInt = FloorToIntSafe(range.maxValue);
            var valueInt = FloorToIntSafe(value);
            return valueInt >= minInt && valueInt <= maxInt;
        }

        private static int CeilToIntSafe(float input)
        {
            if (float.IsNegativeInfinity(input))
            {
                return int.MinValue;
            }
            if (float.IsPositiveInfinity(input))
                return int.MaxValue;
            return Mathf.CeilToInt(input);
        }
        private static int FloorToIntSafe(float input)
        {
            if (float.IsNegativeInfinity(input))
            {
                return int.MinValue;
            }
            if (float.IsPositiveInfinity(input))
                return int.MaxValue;
            return Mathf.FloorToInt(input);
        }

        public BrokenFloatRange Clone()
        {
            return new BrokenFloatRange
            {
                allBoundaries = allBoundaries.ToList(),
                compareDiscrete = compareDiscrete
            };
        }
    }
}
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    [System.Diagnostics.DebuggerDisplay("{ToString(),nq}")]
    public struct GeneSpan
    {
        /// <summary>
        /// the first geneIndex this gene needs space for
        /// </summary>
        public GeneIndex start;
        /// <summary>
        /// the last index this gene takes up space for
        /// </summary>
        public GeneIndex end;

        public static readonly GeneSpan INVALID = new GeneSpan
        {
            start = GeneIndex.INVALID,
            end = GeneIndex.INVALID
        };

        public GeneSpan(GeneIndex singleGene)
        {
            start = singleGene;
            end = singleGene + new GeneIndex(1);
        }
        public GeneSpan(GeneSpan a, GeneSpan b)
        {
            if (a == INVALID)
            {
                if (b == INVALID)
                {
                    start = GeneIndex.INVALID;
                    end = GeneIndex.INVALID;
                }
                else
                {
                    start = b.start;
                    end = b.end;
                }
            }
            else
            {
                if (b == INVALID)
                {
                    start = a.start;
                    end = a.end;
                }
                else
                {
                    start = new GeneIndex(Mathf.Min(a.start.allelePosition, b.start.allelePosition));
                    end = new GeneIndex(Mathf.Max(a.end.allelePosition, b.end.allelePosition));
                }
            }
        }

        public int GetByteLength()
        {
            return ((end - 1).IndexToByteData - start.IndexToByteData + 1);
        }

        public bool CollidesWith(GeneSpan other)
        {
            // if the start of either is contained within the other, must be collision
            if (other.start.allelePosition >= start.allelePosition && other.start.allelePosition < end.allelePosition)
            {
                return true;
            }
            if (start.allelePosition >= other.start.allelePosition && start.allelePosition < other.end.allelePosition)
            {
                return true;
            }
            return false;
        }

        public int Length => end.allelePosition - start.allelePosition;

        public static GeneSpan operator +(GeneSpan a, GeneSpan b)
        {
            return new GeneSpan(a, b);
        }

        public static bool operator ==(GeneSpan a, GeneSpan b)
        {
            return a.start == b.start && a.end == b.end;
        }
        public static bool operator !=(GeneSpan a, GeneSpan b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GeneSpan other))
            {
                return false;
            }
            return other.start == start && other.end == end;
        }

        public override int GetHashCode()
        {
            var hash = start.GetHashCode();
            hash ^= end.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"[{start}, {end})";
        }
    }
}
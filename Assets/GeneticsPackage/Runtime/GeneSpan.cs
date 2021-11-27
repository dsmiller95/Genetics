using System.Collections;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
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

        public GeneSpan(GeneSpan a, GeneSpan b)
        {
            start = new GeneIndex(Mathf.Min(a.start.allelePosition, b.start.allelePosition));
            end = new GeneIndex(Mathf.Max(a.end.allelePosition, b.end.allelePosition));
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
    }
}
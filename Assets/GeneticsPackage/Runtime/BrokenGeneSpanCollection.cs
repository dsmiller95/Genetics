using System.Collections.Generic;

namespace Genetics
{
    public class BrokenGeneSpanCollection
    {
        private SortedList<GeneIndex, GeneSpan> spansOriginSorted;
        public BrokenGeneSpanCollection()
        {
            spansOriginSorted = new SortedList<GeneIndex, GeneSpan>(new DefaultGeneIndexComparer());
        }

        /// <summary>
        /// gets a span which contains all sub-spans in this set
        /// </summary>
        /// <returns>the super span</returns>
        public GeneSpan SuperSpan()
        {
            if (spansOriginSorted.Count <= 0)
            {
                return new GeneSpan()
                {
                    end = GeneIndex.INVALID,
                    start = GeneIndex.INVALID,
                };
            }
            var first = spansOriginSorted.Values[0];
            var min = first.start.allelePosition;
            var max = first.end.allelePosition;

            for (int i = 1; i < spansOriginSorted.Values.Count; i++)
            {
                var nextEnd = spansOriginSorted.Values[i].end.allelePosition;
                if (nextEnd > max)
                {
                    max = nextEnd;
                }
            }

            return new GeneSpan
            {
                start = new GeneIndex(min),
                end = new GeneIndex(max)
            };
        }

        public bool CollidesWith(GeneSpan other)
        {
            foreach (var span in spansOriginSorted.Values)
            {
                if (span.CollidesWith(other))
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(GeneSpan newSpan)
        {
            if (spansOriginSorted.TryGetValue(newSpan.start, out var existingSpan))
            {
                spansOriginSorted[newSpan.start] = new GeneSpan(newSpan, existingSpan);
            }
            else
            {
                spansOriginSorted.Add(newSpan.start, newSpan);
            }
        }

        public void AddAll(BrokenGeneSpanCollection other)
        {
            foreach (var span in other.spansOriginSorted.Values)
            {
                this.Add(span);
            }
        }
    }
}
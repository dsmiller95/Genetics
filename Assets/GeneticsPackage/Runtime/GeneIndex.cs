using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Genetics
{
    [System.Serializable]
    public struct GeneIndex
    {
        public static readonly GeneIndex INVALID = new GeneIndex(-1);

        public int allelePosition;

        public int IndexToByteData => allelePosition / 4;
        public int IndexInsideByte => allelePosition % 4;

        public GeneIndex(int index)
        {
            this.allelePosition = index;
        }

        public override bool Equals(object obj)
        {
            if(obj is GeneIndex indx)
            {
                return indx == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return allelePosition.GetHashCode();
        }

        public static GeneIndex operator +(GeneIndex a, GeneIndex b)
        {
            return new GeneIndex(a.allelePosition + b.allelePosition);
        }

        public static bool operator ==(GeneIndex a, GeneIndex b)
        {
            return a.allelePosition == b.allelePosition;
        }
        public static bool operator !=(GeneIndex a, GeneIndex b)
        {
            return a.allelePosition != b.allelePosition;
        }
    }

    public class DefaultGeneIndexComparer : IComparer<GeneIndex>
    {
        public int Compare(GeneIndex x, GeneIndex y)
        {
            return x.allelePosition.CompareTo(y.allelePosition);
        }
    }
}
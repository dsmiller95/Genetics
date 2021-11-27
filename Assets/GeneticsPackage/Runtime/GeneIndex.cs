using System.Collections;
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
        public static GeneIndex operator +(GeneIndex a, GeneIndex b)
        {
            return new GeneIndex(a.allelePosition + b.allelePosition);
        }
    }
}
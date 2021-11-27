using UnityEngine;

namespace Genetics
{
    public static class HammingUtilities
    {

        /// <summary>
        /// lifted from https://stackoverflow.com/questions/39253221/hamming-weight-of-int64?noredirect=1&lq=1
        /// </summary>
        /// <param name="x"></param>
        /// <returns>varies from 0 to 63</returns>
        public static int HammingWeight(ulong x)
        {
            x = (x & 0x5555555555555555) + ((x >> 1) & 0x5555555555555555); //put count of each  2 bits into those  2 bits 
            x = (x & 0x3333333333333333) + ((x >> 2) & 0x3333333333333333); //put count of each  4 bits into those  4 bits 
            x = (x & 0x0f0f0f0f0f0f0f0f) + ((x >> 4) & 0x0f0f0f0f0f0f0f0f); //put count of each  8 bits into those  8 bits 
            x = (x & 0x00ff00ff00ff00ff) + ((x >> 8) & 0x00ff00ff00ff00ff); //put count of each 16 bits into those 16 bits 
            x = (x & 0x0000ffff0000ffff) + ((x >> 16) & 0x0000ffff0000ffff); //put count of each 32 bits into those 32 bits 
            x = (x & 0x00000000ffffffff) + ((x >> 32) & 0x00000000ffffffff); //put count of each 64 bits into those 64 bits 
            return (int)x;
        }
        public static ulong RandomEvenHammingWeight(System.Random randomGen)
        {
            ulong newGene = 0;
            var binaryProportionalChance = randomGen.NextDouble();
            for (int i = 0; i < sizeof(ulong) * 8; i++)
            {
                var nextBit = randomGen.NextDouble() > binaryProportionalChance;
                if (nextBit)
                {
                    newGene |= ((ulong)1) << i;
                }
            }
            return newGene;
        }

        /// <summary>
        /// Return a boolean value which should be evenly distributed when the bits are evenly distributed
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool EvenSplitHammingWeight(ulong x)
        {
            var weight = HammingWeight(x);
            return (weight % 2) == 0;
        }
    }
}

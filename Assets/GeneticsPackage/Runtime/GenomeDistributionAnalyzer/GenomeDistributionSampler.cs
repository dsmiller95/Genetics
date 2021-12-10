using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics
{

    public class GeneticFloatHistogram
    {
        public GeneticDriver<float> analyzedDriver;
        public int[] buckets;
        public float bucketSeperation;
    }
    public class GeneticBoolHistogram
    {
        public GeneticDriver<bool> analyzedDriver;
        public int falseResultCount;
        public int trueResultCount;
    }

    public class GeneticAnalysisResult
    {
        public GeneticFloatHistogram[] floatResults;
        public GeneticBoolHistogram[] boolResults;
        public int inviable;
    }

    public class GenomeDistributionSampler : ScriptableObject
    {
        public GenomeEditor targetGenome;

        public float defaultHistogramBucketSize;
        public GeneticDriver<float>[] floatsOfInterest;
        public GeneticDriver<bool>[] boolsOfInterest;

        public GeneticAnalysisResult AnalyzeGenome(int samples, System.Random random = null)
        {
            random = random ?? new System.Random();

            var floatBuckets = floatsOfInterest.ToDictionary(x => x.myId, x => new List<int>());
            // first value is count of false values, second is count of true values
            var boolBuckets = boolsOfInterest.ToDictionary(x => x.myId, x => (0, 0));

            var inviables = 0;

            for (int i = 0; i < samples; i++)
            {
                var nextDrivers = targetGenome.CompileGenome(targetGenome.GenerateBaseGenomeData(random));
                if(nextDrivers == null)
                {
                    inviables++;
                    continue;
                }

                foreach (var floatDriver in floatsOfInterest)
                {
                    if(!nextDrivers.TryGetGeneticData(floatDriver, out var value))
                    {
                        Debug.LogWarning($"Genetic data not complete. Missing driver {floatDriver}");
                    }
                    var bucketIndex = Mathf.FloorToInt(value / defaultHistogramBucketSize);
                    var bucketList = floatBuckets[floatDriver.myId];
                    if(bucketList.Count <= bucketIndex)
                    {
                        bucketList.AddRange(Enumerable.Repeat(0, bucketIndex - (bucketList.Count - 1)));
                    }
                    bucketList[bucketIndex]++;
                }

                foreach (var boolDriver in boolsOfInterest)
                {
                    if (!nextDrivers.TryGetGeneticData(boolDriver, out var value))
                    {
                        Debug.LogWarning($"Genetic data not complete. Missing driver {boolDriver}");
                    }
                    var bucketPair = boolBuckets[boolDriver.myId];
                    if (value)
                    {
                        bucketPair.Item2++;
                    }else
                    {
                        bucketPair.Item1++;
                    }
                    boolBuckets[boolDriver.myId] = bucketPair;
                }
            }

            return new GeneticAnalysisResult
            {
                floatResults = floatsOfInterest.Select(x => new GeneticFloatHistogram
                {
                    analyzedDriver = x,
                    buckets = floatBuckets[x.myId].ToArray(),
                    bucketSeperation = this.defaultHistogramBucketSize
                }).ToArray(),
                boolResults = boolsOfInterest.Select(x => new GeneticBoolHistogram
                {
                    analyzedDriver = x,
                    falseResultCount = boolBuckets[x.myId].Item1,
                    trueResultCount = boolBuckets[x.myId].Item2
                }).ToArray()
            };
        }
    }
}
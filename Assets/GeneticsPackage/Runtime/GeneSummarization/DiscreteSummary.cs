using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Genetics.GeneSummarization
{
    public class DiscretSummary : AbstractSummary
    {
        public class BucketClassification
        {
            public string description;
            public int totalClassifications;
            public override string ToString()
            {
                return $"({description} {totalClassifications})";
            }
        }
        public BucketClassification[] allClassifications;

        public DiscretSummary(string[] discreteClassifications, GeneticDriver source) : base(source)
        {
            this.InitializeBuckets(discreteClassifications);
        }
        protected void InitializeBuckets(IEnumerable<string> discreteClassifications)
        {
            allClassifications = discreteClassifications
                .Select(x => new BucketClassification
                {
                    description = x,
                    totalClassifications = 0
                })
                .ToArray();
        }

        /// <summary>
        /// merges any classification classes with exactly the same descriptions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BucketClassification> GetDedupedClasses()
        {
            var classificationsByName = new Dictionary<string, int>();
            foreach (var classification in allClassifications)
            {
                var currentSum = 0;
                classificationsByName.TryGetValue(classification.description, out currentSum);
                classificationsByName[classification.description] = currentSum + classification.totalClassifications;
            }

            foreach (var classification in allClassifications)
            {
                if (classificationsByName.TryGetValue(classification.description, out var totalSum))
                {
                    classificationsByName.Remove(classification.description);
                    yield return new BucketClassification
                    {
                        description = classification.description,
                        totalClassifications = totalSum
                    };
                }
            }
        }

        public override void ClassifyValue(float discreteValue)
        {
            var index = Mathf.FloorToInt(discreteValue);
            if (index >= allClassifications.Length)
            {
                Debug.LogError("value is outside of initialized range of values");
                invalidClassifications++;
                return;
            }
            allClassifications[index].totalClassifications++;
        }
        public override string ToString()
        {
            return string.Join(
                ", ",
                allClassifications
                    .Select(x => x.ToString())
                    .Append($"(unknown {invalidClassifications})")
                );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Genetics.GeneSummarization {
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

        public DiscretSummary(string[] discreteClassifications)
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

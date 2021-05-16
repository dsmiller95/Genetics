using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Genetics.GeneSummarization {
    public abstract class AbstractSummary
    {
        public class BucketClassification
        {
            public string description;
            public int totalClassifications;
        }
        public BucketClassification[] allClassifications;

        protected AbstractSummary()
        {
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

        public virtual void ClassifyValue(float discreteValue)
        {
            var index = Mathf.FloorToInt(discreteValue);
            if (index >= allClassifications.Length)
            {
                Debug.LogError("value is outside of initialized range of values");
                return;
            }
            allClassifications[index].totalClassifications++;
        }
    }
}

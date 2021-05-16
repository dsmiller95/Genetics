using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Genetics.GeneSummarization {
    public class DiscretSummary : AbstractSummary
    {
        public DiscretSummary(string[] discreteClassifications)
        {
            this.InitializeBuckets(discreteClassifications);
        }
    }
}

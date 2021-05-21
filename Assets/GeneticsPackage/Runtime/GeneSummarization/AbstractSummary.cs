using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Genetics.GeneSummarization {
    public abstract class AbstractSummary
    {
        public int invalidClassifications = 0;

        protected AbstractSummary()
        {
        }

        public abstract void ClassifyValue(float discreteValue);
    }
}

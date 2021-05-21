using Genetics.GeneticDrivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Genetics.GeneSummarization
{
    public class GeneticDriverSummarySet
    {
        public Dictionary<string, AbstractSummary> summaries;
        public GeneticDriverSummarySet(
            GeneticDriver[] drivers,
            IEnumerable<CompiledGeneticDrivers> values)
        {
            summaries = drivers.ToDictionary(x => x.DriverName, x => x.GetSummarizer());

            foreach (var valueSet in values)
            {
                for (int driverIndex = 0; driverIndex < drivers.Length; driverIndex++)
                {
                    var driver = drivers[driverIndex];
                    var summarizer = summaries[driver.DriverName];
                    driver.SummarizeValue(summarizer, valueSet);
                }
            }
        }

        public override string ToString()
        {
            var summary = new StringBuilder();
            foreach (var sum in summaries)
            {
                summary.Append($"{sum.Key}: {sum.Value}\n");
            }
            return summary.ToString();
        }
    }
}

using Genetics.GeneSummarization;
using UnityEngine;

namespace Genetics.GeneticDrivers
{
    [CreateAssetMenu(fileName = "BooleanDriver", menuName = "Genetics/BooleanDriver", order = 10)]
    public class BooleanGeneticDriver : GeneticDriver<bool>
    {
        [Tooltip("Used to describe the state of the driver in the UI")]
        public string outcomeWhenTrue;
        [Tooltip("Used to describe the state of the driver in the UI")]
        public string outcomeWhenFalse;
        public override string DescribeState(bool state)
        {
            return state ? outcomeWhenTrue : outcomeWhenFalse;
        }
        public override string DescribeRange(bool min, bool max)
        {
            if(min == max)
            {
                return DescribeState(min);
            }else
            {
                return outcomeWhenTrue + " or " + outcomeWhenFalse;
            }
        }

        public override AbstractSummary GetSummarizer()
        {
            return new DiscretSummary(new string[] { outcomeWhenFalse, outcomeWhenTrue });
        }

        public override void SummarizeValue(AbstractSummary summarizer, CompiledGeneticDrivers valueSet)
        {
            if(valueSet == null || !valueSet.TryGetGeneticData(this, out var value))
            {
                summarizer.invalidClassifications++;
                return;
            }
            summarizer.ClassifyValue(value ? 1f : 0);
        }
    }
}

using Genetics.GeneSummarization;
using UnityEngine;

namespace Genetics.GeneticDrivers
{
    [CreateAssetMenu(fileName = "TextDriver", menuName = "Genetics/TextDriver", order = 10)]
    public class TextGeneticDriver : GeneticDriver<string>
    {
        public override string DescribeState(string state)
        {
            return state;
        }
        public override string DescribeRange(string min, string max)
        {
            if(min.Equals(max))
            {
                return DescribeState(min);
            }else
            {
                return min + " or " + max;
            }
        }

        public override AbstractSummary GetSummarizer()
        {
            Debug.LogWarning("Cannot create summarizer for text genetic driver");
            return null;
        }

        public override void SummarizeValue(AbstractSummary summarizer, CompiledGeneticDrivers valueSet)
        {
            Debug.LogWarning("Cannot summarize text");
        }
    }
}

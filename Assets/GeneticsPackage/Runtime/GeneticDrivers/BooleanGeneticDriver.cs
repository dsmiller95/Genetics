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
    }
}

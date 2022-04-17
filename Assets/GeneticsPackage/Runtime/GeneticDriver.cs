using Dman.ObjectSets;
using Genetics.GeneSummarization;
using Genetics.GeneticDrivers;

namespace Genetics
{
    public abstract class GeneticDriver : IDableObject
    {
        public string DriverName;
        public abstract AbstractSummary GetSummarizer();
        public abstract void SummarizeValue(AbstractSummary summarizer, CompiledGeneticDrivers valueSet);

        public abstract string NameState(object state);

        public override string ToString()
        {
            return DriverName;
        }
    }
    public abstract class GeneticDriver<T> : GeneticDriver
    {
        public abstract string NameState(T state);
        public abstract string DescribeState(T state);
        public abstract string DescribeRange(T min, T max);

        public override string NameState(object state)
        {
            if(state is T casted)
            {
                return NameState(casted);
            }
            return null;
        }
    }
}
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
    }
    public abstract class GeneticDriver<T> : GeneticDriver
    {
        public abstract string DescribeState(T state);
        public abstract string DescribeRange(T min, T max);
    }
}
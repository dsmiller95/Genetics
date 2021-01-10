using Dman.ObjectSets;

namespace Genetics
{
    public abstract class GeneticDriver : IDableObject
    {
        public string DriverName;
    }
    public abstract class GeneticDriver<T> : GeneticDriver
    {
    }
}
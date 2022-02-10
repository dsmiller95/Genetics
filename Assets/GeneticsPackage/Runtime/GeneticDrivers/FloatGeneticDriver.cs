namespace Genetics.GeneticDrivers
{
    /// <summary>
    /// base class for numerical drivers. used to help out unity, since unity doesn't like generics
    /// </summary>
    public abstract class FloatGeneticDriver : GeneticDriver<float>
    {
        /// <summary>
        /// returns whether or not to compare ranges as if they are integer values
        /// </summary>
        /// <returns></returns>
        public abstract bool CompareRangeAsIntegers();
    }
}

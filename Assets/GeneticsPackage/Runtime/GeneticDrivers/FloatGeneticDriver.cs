using UnityEngine;

namespace Genetics.GeneticDrivers
{
    /// <summary>
    /// base class for numerical drivers. used to help out unity, since unity doesn't like generics
    /// </summary>
    public abstract class FloatGeneticDriver : GeneticDriver<float>
    {
        public abstract bool FallsInRange(float min, float max, float value);
    }
}

using Genetics.GeneticDrivers;

namespace Genetics.ParameterizedGenomeGenerator
{
    public interface IGeneticTarget
    {
        public string GetDescriptionOfTarget();
        public bool Matches(CompiledGeneticDrivers drivers);
    }
}

using Genetics.GeneticDrivers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genetics.ParameterizedGenomeGenerator
{
    /// <summary>
    /// Want to define certain parameters within which the genetic drivers must fall.
    /// Naive implementation will be randomly generating values, and filtering out those which
    ///     do not match filter parameters
    /// binary serialization compatabile and unity inspector compatible
    /// </summary>
    [System.Serializable]
    public class GenomeGenerator
    {
        public BooleanGeneticTarget[] booleanTargets;
        public FloatGeneticTarget[] floatTargets;
        public GenomeEditor genomeTarget;

        /// <summary>
        /// Generate genomes infinitely. Will retry randomly generating genomes, and only return those which match the
        ///     target definitions. Will only run the generation algorythm <paramref name="nullProcessingSpacer"/> times
        ///     before yielding a null. In this way, null values can be used to spae out 
        /// </summary>
        /// <param name="randomSource"></param>
        /// <param name="nullProcessingSpacer"></param>
        /// <returns>An infinite series of genomes which match the target params, and nulls to space out results based on processing time</returns>
        public IEnumerable<Genome> GenerateGenomes(Random randomSource, int nullProcessingSpacer)
        {
            var processingSinceLastSpacer = 0;
            while (true)
            {
                var nextGenome = genomeTarget.GenerateBaseGenomeData(randomSource);
                var nextDrivers = genomeTarget.CompileGenome(nextGenome);
                processingSinceLastSpacer++;
                if (DriversMatch(nextDrivers))
                {
                    yield return nextGenome;
                }
                if (processingSinceLastSpacer >= nullProcessingSpacer)
                {
                    yield return null;
                    processingSinceLastSpacer = 0;
                }
            }
        }

        private bool DriversMatch(CompiledGeneticDrivers drivers)
        {
            return booleanTargets
                .Cast<IGeneticTarget>()
                .Concat(floatTargets)
                .All(x => x.Matches(drivers));
        }
    }
}
